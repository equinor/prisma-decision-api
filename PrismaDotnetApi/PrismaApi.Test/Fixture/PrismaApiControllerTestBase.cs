namespace PrismaApi.Test.Fixture;

public abstract class PrismaApiControllerTestBase : IAsyncLifetime
{
    protected PrismaApiControllerTestBase(PrismaApiFixture fixture)
    {
        Fixture = fixture;
    }

    protected PrismaApiFixture Fixture { get; }

    public Task InitializeAsync() => Fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
