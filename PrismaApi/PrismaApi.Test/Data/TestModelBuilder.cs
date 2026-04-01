using Microsoft.Extensions.DependencyInjection;
using PrismaApi.Test.Fixture;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Test.Data;

public class TestModelBuilder
{
    internal static async Task<TestArguments> BuildFreshTestDataAsync(PrismaApiFixture fixture)
    {
        using var scope = fixture.ApiFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        var args = new TestArguments();

        return args;
    }
}

public class TestArguments
{

    public int TestProjectId { get; set; } = GenerateUniqueId();

    public static int GenerateUniqueId()
    {
        var ticks = DateTime.UtcNow.Ticks;
        var randomNumber = new Random().Next();
        return (int)((ticks / TimeSpan.TicksPerSecond) ^ randomNumber);
    }
}
