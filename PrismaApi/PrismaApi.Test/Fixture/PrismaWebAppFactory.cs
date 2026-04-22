using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
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
    private readonly SqliteConnection _connection;
    private readonly string _testDbConnectionString = "DataSource=:memory:";

    public PrismaWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        IdentityModelEventSource.ShowPII = true;

        Environment.SetEnvironmentVariable("SqlDb:ConnectionString", _testDbConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings:SqliteConnection", _testDbConnectionString);

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
            options.UseSqlite(_connection);
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
                options.UseSqlite(_connection);
            });


        });
        base.ConfigureWebHost(builder);
    }
}
