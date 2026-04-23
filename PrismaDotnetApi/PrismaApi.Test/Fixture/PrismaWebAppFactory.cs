using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using PrismaApi.Api;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Services;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Test.Configuration.Constants;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Mocks;

namespace PrismaApi.Test.Fixture;

public class PrismaWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"Database=sqldb-ep-Prisma-intgtest-{DateTimeOffset.Now:yyyy-MM-dd-HHmmss}-{Guid.NewGuid()};";

    private readonly string _testDbConnectionString =
        Environment.GetEnvironmentVariable("GITHUB_TEST_CONNECTION_STRING") ??
        "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;Max Pool Size = 32767;Pooling=true;";

    public PrismaWebAppFactory()
    {
        _testDbConnectionString += _databaseName;

        IdentityModelEventSource.ShowPII = true;

        Environment.SetEnvironmentVariable("SqlDb:ConnectionString", _testDbConnectionString);

        Environment.SetEnvironmentVariable(IntegrationTestEnvVariables.IntegrationTestMarker,
            "true");
        Environment.SetEnvironmentVariable("AzureAd__ClientId", $"{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("FORWARD_JWT", "True");
        Environment.SetEnvironmentVariable("FORWARD_COOKIE", "True");

        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(_testDbConnectionString);
        });

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
        dbContext.Database.EnsureCreated();
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddIntegrationTestingAuthentication();
            services.RemoveAll<IUserService>();
            services.RemoveAll<IUserProvider>();
            services.AddScoped<IUserProvider, InternalUserService>();
            services.AddScoped<IUserService, TestUserService>();
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(_testDbConnectionString);
            });


        });
        base.ConfigureWebHost(builder);
    }
}
