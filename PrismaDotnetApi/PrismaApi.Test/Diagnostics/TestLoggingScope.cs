using Xunit.Abstractions;

namespace PrismaApi.Test.Diagnostics;

public class TestLoggingScope : IDisposable
{
    private static readonly AsyncLocal<ITestOutputHelper> Logger = new();

    public TestLoggingScope(ITestOutputHelper logger) => Logger.Value = logger;

    public static ITestOutputHelper Current => Logger.Value!;

    public void Dispose()
    {
        Logger.Value = null!;
        GC.SuppressFinalize(this);
    }
}
