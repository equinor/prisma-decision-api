using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using PrismaApi.Test.Configuration.Schemas.IntegrationTest;

namespace PrismaApi.Test.Configuration.Extensions;

public static class AuthenticationBuilderExtensions
{
    public static IServiceCollection AddIntegrationTestingAuthentication(
        this IServiceCollection services)
    {
        var builder = services.AddAuthentication();

        builder.AddScheme<IntegrationTestAuthOptions, IntegrationTestAuthHandler>(
            IntegrationTestAuthDefaults.AuthenticationScheme, opts => { });

        if (Environment.GetEnvironmentVariable("FORWARD_JWT") != null)
        {
            services.PostConfigureAll<JwtBearerOptions>(o =>
                o.ForwardAuthenticate = IntegrationTestAuthDefaults.AuthenticationScheme);
        }

        if (Environment.GetEnvironmentVariable("FORWARD_COOKIE") != null)
        {
            services.PostConfigureAll<CookieAuthenticationOptions>(o =>
                o.ForwardAuthenticate = IntegrationTestAuthDefaults.AuthenticationScheme);
        }

        return services;
    }
}
