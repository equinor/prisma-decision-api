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
            AzureUniqueId = testUser.Id,
            Name = testUser.Name,
            Upn = testUser.Mail,
            Roles = testUser.Roles,
            Scopes = testUser.Scopes,
            IsAppToken = false,
            AppId = appId
        };

        message.Headers.Add("Authorization", AuthTokenUtilities.WrapAuthToken(testToken));
    }

    #region OPTIONS

    public static async Task<TestClientHttpResponse<dynamic>> TestClientOptionsAsync(
        this HttpClient client,
        string requestUri)
    {
        var message = new HttpRequestMessage(HttpMethod.Options, requestUri);

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<dynamic>.CreateResponseAsync<dynamic>(resp);

        TestLogger.TryLog($"OPTIONS {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    #endregion OPTIONS

    #region GET

    public static async Task<TestClientHttpResponse<TResp>> TestClientGetAsync<TResp>(
        this HttpClient client,
        string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<TResp>.CreateResponseAsync<TResp>(resp);

        TestLogger.TryLog($"GET {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            TestLogger.TryLog(respObj.Content);
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<TResp>> TestClientGetAsync<TResp>(
        this HttpClient client,
        string requestUri, TResp returnType)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<TResp>.CreateResponseAsync(resp, returnType);

        TestLogger.TryLog($"GET {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            TestLogger.TryLog(respObj.Content);
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<string>> TestClientGetStringAsync(
        this HttpClient client,
        string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<string>.CreateResponseAsync<string>(resp, true);

        TestLogger.TryLog($"GET {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            TestLogger.TryLog(respObj.Content);
        }

        return respObj;
    }

    #endregion GET

    #region POST

    public static async Task<TestClientHttpResponse<T>> TestClientPostAsync<T>(
        this HttpClient client,
        string requestUri, object value)
    {
        var content = JsonSerializer.Serialize(value);

        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<T>.CreateResponseAsync<T>(resp);

        TestLogger.TryLog($"POST {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<dynamic>> TestClientPostAsync(
        this HttpClient client,
        string requestUri, object value)
    {
        var content = JsonSerializer.Serialize(value);

        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<dynamic>.CreateResponseAsync<dynamic>(resp);

        TestLogger.TryLog($"POST {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<T>> TestClientPostAsync<T>(
        this HttpClient client,
        string requestUri, object value, T responseType)
    {
        var content = JsonSerializer.Serialize(value);

        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<T>.CreateResponseAsync(resp, responseType);

        TestLogger.TryLog($"POST {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<T>> TestClientPostNoPayloadAsync<T>(
        this HttpClient client,
        string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<T>.CreateResponseAsync<T>(resp);

        TestLogger.TryLog($"POST {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<T>> TestClientPostNoPayloadAsync<T>(
        this HttpClient client,
        string requestUri, T returnType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<T>.CreateResponseAsync(resp, returnType);

        TestLogger.TryLog($"POST {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<TResp>> TestClientPostMultipartAsync<TResp>(
        this HttpClient client,
        string requestUri, MultipartFormDataContent form, TResp respType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = form };

        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<TResp>.CreateResponseAsync(resp, respType);

        TestLogger.TryLog($"POST {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            TestLogger.TryLog(respObj.Content);
            TestLogger.TryLog("Request payload: -- form data --");
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<TResponse>> TestClientPostFileAsync<TResponse>(
        this HttpClient client, string requestUri, Stream documentStream, string contentType,
        TResponse respType)
    {
        using var streamContent = new StreamContent(documentStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        streamContent.Headers.ContentLength = documentStream.Length;

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = streamContent
        };
        TestClientScope.AddHeaders(request);

        var resp = await client.SendAsync(request);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync(resp, respType);

        TestLogger.TryLog(
            $"POST {resp.RequestMessage?.RequestUri} [CT: {contentType}] -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    #endregion POST

    #region PATCH

    public static async Task<TestClientHttpResponse<TResponse>> TestClientPatchAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value)
    {
        var content = JsonSerializer.Serialize(value);
        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var message = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync<TResponse>(resp);

        TestLogger.TryLog($"PATCH {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<TResponse>> TestClientPatchAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value, TResponse response)
    {
        var content = JsonSerializer.Serialize(value);
        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var message = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync(resp, response);

        TestLogger.TryLog($"PATCH {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    #endregion PATCH

    #region PUT

    public static async Task<TestClientHttpResponse<TResponse>> TestClientPutAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value)
    {
        var content = JsonSerializer.Serialize(value);
        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var message = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync<TResponse>(resp);

        TestLogger.TryLog($"PUT {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<TResponse>> TestClientPutAsync<TResponse>(
        this HttpClient client,
        string requestUri, object value, TResponse response)
    {
        var content = JsonSerializer.Serialize(value);
        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

        var message = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = stringContent
        };

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync(resp, response);

        TestLogger.TryLog($"PUT {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
            TestLogger.TryLog($"Request payload: {content}");
        }

        return respObj;
    }

    #endregion PUT

    #region DELETE

    public static async Task<TestClientHttpResponse<TResponse>> TestClientDeleteAsync<TResponse>(
        this HttpClient client,
        string requestUri)
    {
        var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync<TResponse>(resp);

        TestLogger.TryLog($"DELETE {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<TResponse>> TestClientDeleteAsync<TResponse>(
        this HttpClient client,
        string requestUri, TResponse response)
    {
        var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<TResponse>.CreateResponseAsync(resp, response);

        TestLogger.TryLog($"DELETE {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    public static async Task<TestClientHttpResponse<dynamic>> TestClientDeleteAsync(
        this HttpClient client,
        string requestUri)
    {
        var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        TestClientScope.AddHeaders(message);

        var resp = await client.SendAsync(message);
        var respObj = await TestClientHttpResponse<dynamic>.CreateResponseAsync<dynamic>(resp);

        TestLogger.TryLog($"DELETE {resp.RequestMessage?.RequestUri} -> {resp.StatusCode}");
        if (!resp.IsSuccessStatusCode)
        {
            var respContent = await resp.Content.ReadAsStringAsync();
            TestLogger.TryLog(respContent);
        }

        return respObj;
    }

    #endregion DELETE
}
