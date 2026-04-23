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
        var connectionString = config.GetSection("ConnectionStrings")["DefaultConnection"];
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        Console.WriteLine($"Using connection string: {connectionString}");

        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
