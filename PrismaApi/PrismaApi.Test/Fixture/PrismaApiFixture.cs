using Microsoft.Extensions.DependencyInjection;
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

        WarpUser = new TestPersonProfile
        {
            AzureUniqueId = Guid.NewGuid(),
            Name = "Test User A",
            Mail = "noreplyTestA@mail.com",
            Scopes = new[] { "Read" },
            Roles = new[] { "PrismaUser" }
        };
    }

    public TestArguments TestArgs { get; set; } = new();

    public TestPersonProfile WarpUser { get; }
    public TestPersonProfile RandomUser { get; }
    public TestPersonProfile AdminUser { get; }
    public TestPersonProfile FieldASpecificEditorUser { get; }
    public TestPersonProfile FieldASpecificReadOnlyUser { get; }
    public TestPersonProfile FieldBSpecificReadOnlyUser { get; }
    public TestPersonProfile FieldBSpecificEditorUser { get; }
    public TestPersonProfile NoFieldAccessUser { get; }
    public TestPersonProfile AsgardUser { get; }

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

    public TestClientScope UserScope() => new(WarpUser);

    public TestClientScope RandomUserScope() => new(RandomUser);

    public TestClientScope AdminScope() => new(AdminUser);

    public TestClientScope FieldASpecificEditorScope() => new(FieldASpecificEditorUser);

    public TestClientScope FieldASpecificReadOnlyScope() => new(FieldASpecificReadOnlyUser);

    public TestClientScope NoFieldAccessScope() => new(NoFieldAccessUser);

    public TestClientScope FieldBSpecificReadOnlyScope() => new(FieldBSpecificReadOnlyUser);

    public TestClientScope FieldBSpecificEditorScope() => new(FieldBSpecificEditorUser);

    public TestClientScope AsgardScope() => new(AsgardUser);
}
