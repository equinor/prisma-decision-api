using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Configuration.User;

namespace PrismaApi.Test.Configuration;

public class TestClientScope : IDisposable
{
    private static readonly AsyncLocal<List<KeyValuePair<string, string>>> CurrentHeaders = new();
    private static readonly AsyncLocal<TestPersonProfile> CurrentUser = new();
    private static readonly AsyncLocal<Guid?> CurrentAppId = new();

    public TestClientScope(Guid id) => CurrentAppId.Value = id;

    public TestClientScope(TestPersonProfile profile) => CurrentUser.Value = profile;

    public void Dispose() => GC.SuppressFinalize(this);

    public TestClientScope SetSigninAppId(Guid? appId)
    {
        CurrentAppId.Value = appId;
        return this;
    }

    public static void AddHeaders(HttpRequestMessage message)
    {
        if (CurrentHeaders.Value != null)
        {
            foreach (var header in CurrentHeaders.Value)
            {
                message.Headers.Add(header.Key, header.Value);
            }
        }

        if (CurrentUser.Value != null)
        {
            message.AddTestUserToken(CurrentUser.Value, CurrentAppId.Value);
        }
    }
}
