using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PrismaApi.Infrastructure.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        var apiPath = Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                "PrismaApi.Api"
            );
        configurationBuilder
            .SetBasePath(apiPath)
            .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);

        IConfiguration config = configurationBuilder.Build();
        var connectionString = IsSqlServer(args)
            ? config.GetSection("ConnectionStrings")["DefaultConnection"]
            : config.GetSection("ConnectionStrings")["SqliteConnection"];

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        Console.WriteLine($"Using connection string: {connectionString}");

        var provider = GetProvider(args);

        if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString,
                x => x.MigrationsAssembly("SqliteMigrations"));
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString,
                x => x.MigrationsAssembly("SqlServerMigrations"));
        }

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string GetProvider(string[] args)
    {
        var providerArg = Array.Find(args, a => a.StartsWith("--provider=", StringComparison.OrdinalIgnoreCase));
        return providerArg?.Split('=')[1] ?? "sqlserver";
    }

    private static bool IsSqlServer(string[] args)
    {
        return GetProvider(args).Equals("sqlserver", StringComparison.OrdinalIgnoreCase);
    }
}
