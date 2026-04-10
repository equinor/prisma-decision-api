using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;

namespace PrismaApi.Api.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwagger(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSwaggerGen(c =>
        {
            var tenantId = configuration.GetValue<string>("AzureAd:TenantId");
            var audience = configuration.GetValue<string>("AzureAd:Audience");
            const string scope = "Read";
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Prisma API", Version = "v1" });

            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //c.IncludeXmlComments(xmlPath);

            c.SupportNonNullableReferenceTypes();
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl =
                            new Uri(
                                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri(
                            $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string> { { $"{audience}/{scope}", scope } }
                    }
                }
            });

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "oauth2", Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            };

            c.AddSecurityRequirement(securityRequirement);
        });
    public static void UseSwaggerWithAuth(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Prisma API v1");
            options.OAuthClientId(configuration.GetValue<string>("AzureAd:ClientId"));
            //options.OAuthClientSecret(configuration.GetValue<string>("AzureAd:ClientSecret"));
            options.OAuthUsePkce();
        });
    }
    public static IServiceCollection AddPrismaRateLimiter(this IServiceCollection services,
        bool notRunningIntegrationTests = true)
    {
        const int defaultLimit = 150;

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.User.Identity?.Name ??
                        httpContext.Request.Headers.Host.ToString(),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = defaultLimit,
                            QueueLimit = defaultLimit * 2,
                            Window = notRunningIntegrationTests
                                ? TimeSpan.FromSeconds(2)
                                : TimeSpan.FromSeconds(30)
                        }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 429;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    await context.HttpContext.Response.WriteAsync($"Too many requests. Please try again after {retryAfter.TotalSeconds} second(s).", cancellationToken: token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
                }
            };
        });
        return services;
    }
}
