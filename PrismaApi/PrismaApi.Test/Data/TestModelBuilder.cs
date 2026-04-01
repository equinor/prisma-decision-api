using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.Data;

public class TestModelBuilder
{
    internal static async Task<TestArguments> BuildFreshTestDataAsync(PrismaApiFixture fixture)
    {
        using var scope = fixture.ApiFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var args = new TestArguments();

        db.Users.Add(new Domain.Entities.User
        {
            Id = fixture.PrismaUser.Id!,
            Name = fixture.PrismaUser.Name!
        });

        var project = new Project
        {
            Id = args.TestProjectId,
            Name = "Test Project",
            CreatedById = fixture.PrismaUser.Id!,
            UpdatedById = fixture.PrismaUser.Id!,
        };


        return args;
    }
}

public class TestArguments
{

    public Guid TestProjectId { get; set; } = Guid.NewGuid();

    public static int GenerateUniqueId()
    {
        var ticks = DateTime.UtcNow.Ticks;
        var randomNumber = new Random().Next();
        return (int)((ticks / TimeSpan.TicksPerSecond) ^ randomNumber);
    }
}
