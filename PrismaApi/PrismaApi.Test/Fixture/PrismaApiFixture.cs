using Microsoft.Extensions.DependencyInjection;
using PrismaApi.Domain.Constants;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Test.Configuration;
using PrismaApi.Test.Configuration.User;
using PrismaApi.Test.Data;

namespace PrismaApi.Test.Fixture;

public class PrismaApiFixture : IAsyncLifetime
{
    public readonly PrismaWebAppFactory ApiFactory;

    public PrismaApiFixture()
    {
        ApiFactory = new PrismaWebAppFactory();

        PrismaUser = new TestPersonProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test User A",
            Mail = "noreplyTestA@mail.com",
            Scopes = new[] { "Read" },
            Roles = new[] { AppRoles.PrismaDecisionUser }
        };

        SecundaryUser = new TestPersonProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test User A",
            Mail = "noreplyTestA@mail.com",
            Scopes = new[] { "Read" },
            Roles = new[] { AppRoles.PrismaDecisionUser }
        };
    }

    public TestArguments TestArgs { get; set; } = new();

    public TestPersonProfile PrismaUser { get; }
    public TestPersonProfile SecundaryUser { get; }
    
    public async Task InitializeAsync() =>
        TestArgs = await TestModelBuilder.BuildFreshTestDataAsync(this);

    public async Task DisposeAsync()
    {
        using var scope = ApiFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();

        GC.SuppressFinalize(db);
        await Task.CompletedTask;
    }

    public TestClientScope UserScope() => new(PrismaUser);
}
