using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PrismaApi.Api.Configuration.Extensions;
using PrismaApi.Api.Configuration.JsonResponseOptions;
using PrismaApi.Api.SecurityPolicy;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Repositories;
using PrismaApi.Application.Services;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.DiscreteTables;
using PrismaApi.Infrastructure.Interfaces;

namespace PrismaApi.Api;

public class Program
{
    private static bool NotRunningIntegrationTests = !string.Equals(
        Environment.GetEnvironmentVariable("INTEGRATION_TEST_RUN"), "true",
        StringComparison.OrdinalIgnoreCase);

    public static void Main(string[] args)
    {


        var builder = WebApplication.CreateBuilder(args);

        var isPublicInstance = builder.Environment.EnvironmentName.Equals("Public", StringComparison.OrdinalIgnoreCase);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        if (!string.IsNullOrEmpty(clientSecret))
        {
            builder.Configuration["AzureAd:ClientSecret"] = clientSecret;
        }
        if (!isPublicInstance)
        {

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration, "AzureAd")
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi("FastApi", builder.Configuration.GetSection("FastApiService"))
                .AddMicrosoftGraph(builder.Configuration.GetSection("GraphApi"))
                .AddInMemoryTokenCaches();
        }
        else
        {

            builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi("FastApi", builder.Configuration.GetSection("FastApiService"))
                .AddInMemoryTokenCaches();
        }

        builder.Services.AddMemoryCache();

        var appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATION_INSIGHTS_CONNECTIONSTRING") ?? builder.Configuration.GetSection("ApplicationInsights:ConnectionString").Value;
        if (!string.IsNullOrEmpty(appInsightsConnectionString) && builder.Environment.EnvironmentName != "Local")
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
        }

        builder.Services
            .AddPrismaRateLimiter(NotRunningIntegrationTests);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection");

        if (!string.IsNullOrEmpty(sqliteConnectionString))
        {
            builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite(sqliteConnectionString, 
                    x => x.MigrationsAssembly("SqliteMigrations"));
            });
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString, 
                    x => x.MigrationsAssembly("SqlServerMigrations"));
            });
        }

        builder.Services.AddScoped<IDiscreteTableRuleEventHandler, DiscreteTableRuleEventHandler>();
        builder.Services.AddScoped<IIssueRepository, IssueRepository>();
        builder.Services.AddScoped<INodeRepository, NodeRepository>();
        builder.Services.AddScoped<INodeStyleRepository, NodeStyleRepository>();
        builder.Services.AddScoped<IEdgeRepository, EdgeRepository>();
        builder.Services.AddScoped<IDecisionRepository, DecisionRepository>();
        builder.Services.AddScoped<IOptionRepository, OptionRepository>();
        builder.Services.AddScoped<IOutcomeRepository, OutcomeRepository>();
        builder.Services.AddScoped<IUncertaintyRepository, UncertaintyRepository>();
        builder.Services.AddScoped<IUtilityRepository, UtilityRepository>();
        builder.Services.AddScoped<IDiscreteProbabilityRepository, DiscreteProbabilityRepository>();
        builder.Services.AddScoped<IDiscreteUtilityRepository, DiscreteUtilityRepository>();
        builder.Services.AddScoped<IValueMetricRepository, ValueMetricRepository>();
        builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();
        builder.Services.AddScoped<IObjectiveRepository, ObjectiveRepository>();
        builder.Services.AddScoped<IProjectRoleRepository, ProjectRoleRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IProjectDuplicationRepository, ProjectDuplicationRepository>();
        builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        builder.Services.AddScoped<IDecisionQualityAssessmentRepository, DecisionQualityAssessmentRepository>();

        builder.Services.AddScoped<ITableRebuildingService, TableRebuildingService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IIssueService, IssueService>();
        builder.Services.AddScoped<INodeService, NodeService>();
        builder.Services.AddScoped<INodeStyleService, NodeStyleService>();
        builder.Services.AddScoped<IEdgeService, EdgeService>();
        builder.Services.AddScoped<IDecisionService, DecisionService>();
        builder.Services.AddScoped<IOptionService, OptionService>();
        builder.Services.AddScoped<IOutcomeService, OutcomeService>();
        builder.Services.AddScoped<IUncertaintyService, UncertaintyService>();
        builder.Services.AddScoped<IUtilityService, UtilityService>();
        builder.Services.AddScoped<IDiscreteProbabilityService, DiscreteProbabilityService>();
        builder.Services.AddScoped<IDiscreteUtilityService, DiscreteUtilityService>();
        builder.Services.AddScoped<IValueMetricService, ValueMetricService>();
        builder.Services.AddScoped<IStrategyService, StrategyService>();
        builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
        builder.Services.AddScoped<IProjectRoleService, ProjectRoleService>();

        if (isPublicInstance)
        {
            builder.Services.AddScoped<IUserProvider, PublicUserService>();
        }
        else
        {
            builder.Services.AddScoped<IUserProvider, InternalUserService>();
        }

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IProjectDuplicationService, ProjectDuplicationService>();
        builder.Services.AddScoped<IProjectImportService, ProjectImportService>();
        builder.Services.AddScoped<IAssessmentService, AssessmentService>();
        builder.Services.AddScoped<IDecisionQualityAssessmentService, DecisionQualityAssessmentService>();
        builder.Services.AddHttpClient<IFastApiService, FastApiService>();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new DateTimeOffsetJsonConverter());
            });

        builder.Services.AddSwagger(builder.Configuration);
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AppRolesPolicy.UserRoleRequired, policy =>
            {
                if (isPublicInstance)
                {
                    policy.RequireAssertion(_ => true);
                }
                else
                {
                    AppRolesPolicy.AddPrismaDecisionUserPolicy(policy);
                }
            });
        });

        string[] allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!;
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                CorsPolicy.AllowOriginsPolicy,
                policy => CorsPolicy.AddCorsPolicy(policy, allowedOrigins)
            );
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
        {
            app.UseSwagger();
            app.UseSwaggerWithAuth(builder.Configuration);
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRateLimiter();

        // Use CORS - must be before UseAuthorization
        app.UseCors(CorsPolicy.AllowOriginsPolicy);

        if (!isPublicInstance)
        {
            app.UseAuthorization();
        }

        app.MapControllers();

        if (!string.IsNullOrEmpty(sqliteConnectionString))
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
        }
        app.Run();

    }
}
