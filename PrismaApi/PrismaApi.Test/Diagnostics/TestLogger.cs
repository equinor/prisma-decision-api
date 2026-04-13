using System.Text.Json;

namespace PrismaApi.Test.Diagnostics;

public class TestLogger
{
    public static void TryLog(string message) => TestLoggingScope.Current?.WriteLine(message);

    public static void TryLogObject(object message) =>
        TestLoggingScope.Current?.WriteLine(JsonSerializer.Serialize(message));
}
