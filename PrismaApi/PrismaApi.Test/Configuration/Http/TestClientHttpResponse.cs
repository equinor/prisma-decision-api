using System.Text.Json;
using System.Text.Json.Serialization;
using PrismaApi.Test.Diagnostics;

namespace PrismaApi.Test.Configuration.Http;

public class TestClientHttpResponse<TResp>
{
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = false
    };

    public TestClientHttpResponse(HttpResponseMessage response)
    {
        Response = response;
        HttpReason = response.ReasonPhrase!;
    }

    public string Content { get; set; } = null!;
    public HttpResponseMessage Response { get; set; } = null!;
    public string HttpReason { get; set; } = null!;

    public TResp Value { get; set; } = default!;

    public static async Task<TestClientHttpResponse<T>> CreateResponseAsync<T>(
        HttpResponseMessage response,
        bool skipDeserialization = false)
    {
        var respObject = new TestClientHttpResponse<T>(response)
        {
            Content = await response.Content.ReadAsStringAsync()
        };

        try
        {
            if (skipDeserialization && typeof(T) == typeof(string))
            {
                respObject.Value = (T) (object) respObject.Content;
            }
            else
            {
                respObject.Value =
                    JsonSerializer.Deserialize<T>(respObject.Content, SerializerOptions)!;
            }
        }
        catch (Exception ex)
        {
            TestLogger.TryLog($"Unable to deserialize to type '{typeof(T).Name}'");
            TestLogger.TryLog(ex.ToString());
        }

        return respObject;
    }

    public static async Task<TestClientHttpResponse<T>> CreateResponseAsync<T>(
        HttpResponseMessage response,
        T anonymousType)
    {
        var respObject = new TestClientHttpResponse<T>(response)
        {
            Content = await response.Content.ReadAsStringAsync()
        };

        try
        {
            respObject.Value =
                JsonSerializer.Deserialize<T>(respObject.Content, SerializerOptions)!;
        }
        catch (Exception ex)
        {
            TestLogger.TryLog($"Unable to deserialize to type '{typeof(T).Name}'");
            TestLogger.TryLog(ex.ToString());
        }

        return respObject;
    }
}
