using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PrismaApi.Test.Configuration.Http;
using PrismaApi.Test.Configuration.User;
using PrismaApi.Test.Diagnostics;

namespace PrismaApi.Test.Configuration.Extensions;

public static class TestHttpClientExtensions
{
    public static void AddTestUserToken(this HttpRequestMessage message, TestPersonProfile testUser,
        Guid? appId)
    {
        var testToken = new TestToken
        {
            Id = testUser.Id,
            Name = testUser.Name,
            Upn = testUser.Mail,
            Roles = testUser.Roles,
            Scopes = testUser.Scopes,
            IsAppToken = false,
            AppId = appId
        };

        message.Headers.Add("Authorization", AuthTokenUtilities.WrapAuthToken(testToken));
    }

    private static async Task<TestClientHttpResponse<TResp>> SendAsync<TResp>(
        this HttpClient client,
        HttpMethod method,
        string requestUri,
        object? payload = null,
        bool skipDeserialization = false)
    {
        var request = new HttpRequestMessage(method, requestUri);
        
        if (payload is HttpContent httpContent)
        {
            request.Content = httpContent;
        }
        else if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<TResp>.CreateResponseAsync<TResp>(resp, skipDeserialization);

        TestLogger.TryLog($"{method.Method} {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            TestLogger.TryLog(respObj.Content);
        }
        if (!resp.IsSuccessStatusCode && payload != null && payload is not HttpContent)
        {
            TestLogger.TryLog($"Request payload: {JsonSerializer.Serialize(payload)}");
        }

        return respObj;
    }

    #region OPTIONS

    public static Task<TestClientHttpResponse<dynamic>> TestClientOptionsAsync(
        this HttpClient client,
        string requestUri)
        => client.SendAsync<dynamic>(HttpMethod.Options, requestUri);

    #endregion OPTIONS

    #region GET

    public static Task<TestClientHttpResponse<TResp>> TestClientGetAsync<TResp>(
        this HttpClient client,
        string requestUri)
        => client.SendAsync<TResp>(HttpMethod.Get, requestUri);

    public static Task<TestClientHttpResponse<TResp>> TestClientGetAsync<TResp>(
        this HttpClient client,
        string requestUri, TResp returnType)
        => client.SendAsync<TResp>(HttpMethod.Get, requestUri);

    public static Task<TestClientHttpResponse<string>> TestClientGetStringAsync(
        this HttpClient client,
        string requestUri)
        => client.SendAsync<string>(HttpMethod.Get, requestUri, skipDeserialization: true);

    #endregion GET

    #region POST

    public static Task<TestClientHttpResponse<T>> TestClientPostAsync<T>(
        this HttpClient client,
        string requestUri, object value)
        => client.SendAsync<T>(HttpMethod.Post, requestUri, value);

    public static Task<TestClientHttpResponse<dynamic>> TestClientPostAsync(
        this HttpClient client,
        string requestUri, object value)
        => client.SendAsync<dynamic>(HttpMethod.Post, requestUri, value);

    public static Task<TestClientHttpResponse<T>> TestClientPostAsync<T>(
        this HttpClient client,
        string requestUri, object value, T responseType)
        => client.SendAsync<T>(HttpMethod.Post, requestUri, value);

    public static Task<TestClientHttpResponse<T>> TestClientPostNoPayloadAsync<T>(
        this HttpClient client,
        string requestUri)
        => client.SendAsync<T>(HttpMethod.Post, requestUri);

    public static Task<TestClientHttpResponse<T>> TestClientPostNoPayloadAsync<T>(
        this HttpClient client,
        string requestUri, T returnType)
        => client.SendAsync<T>(HttpMethod.Post, requestUri);

    public static Task<TestClientHttpResponse<TResp>> TestClientPostMultipartAsync<TResp>(
        this HttpClient client,
        string requestUri, MultipartFormDataContent form, TResp respType)
        => client.SendAsync<TResp>(HttpMethod.Post, requestUri, form);

    public static async Task<TestClientHttpResponse<TResponse>> TestClientPostFileAsync<TResponse>(
        this HttpClient client, string requestUri, Stream documentStream, string contentType,
        TResponse respType)
    {
        using var streamContent = new StreamContent(documentStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        streamContent.Headers.ContentLength = documentStream.Length;
        return await client.SendAsync<TResponse>(HttpMethod.Post, requestUri, streamContent);
    }

    #endregion POST

    #region PATCH

    public static Task<TestClientHttpResponse<TResponse>> TestClientPatchAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value)
        => client.SendAsync<TResponse>(HttpMethod.Patch, requestUri, value);

    public static Task<TestClientHttpResponse<TResponse>> TestClientPatchAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value, TResponse response)
        => client.SendAsync<TResponse>(HttpMethod.Patch, requestUri, value);

    #endregion PATCH

    #region PUT

    public static Task<TestClientHttpResponse<TResponse>> TestClientPutAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value)
        => client.SendAsync<TResponse>(HttpMethod.Put, requestUri, value);

    public static Task<TestClientHttpResponse<TResponse>> TestClientPutAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value, TResponse response)
        => client.SendAsync<TResponse>(HttpMethod.Put, requestUri, value);

    #endregion PUT

    #region DELETE

    public static Task<TestClientHttpResponse<TResponse>> TestClientDeleteAsync<TResponse>(
        this HttpClient client,
        string requestUri)
        => client.SendAsync<TResponse>(HttpMethod.Delete, requestUri);

    public static Task<TestClientHttpResponse<TResponse>> TestClientDeleteAsync<TResponse>(
        this HttpClient client,
        string requestUri, TResponse response)
        => client.SendAsync<TResponse>(HttpMethod.Delete, requestUri);

    public static Task<TestClientHttpResponse<dynamic>> TestClientDeleteAsync(
        this HttpClient client,
        string requestUri)
        => client.SendAsync<dynamic>(HttpMethod.Delete, requestUri);

    #endregion DELETE
}
