using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Net;
using System.Net.Http.Headers;

using PrismaApi.Application.Interfaces;

namespace PrismaApi.Application.Services;

public class FastApiService : IFastApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;
    public FastApiService(HttpClient httpClient, IConfiguration configuration, ITokenAcquisition tokenAcquisition)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(180);
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
    }
    public class ApiResponse
    {
        public string? Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public async Task<ApiResponse> CallDownstreamFastApiGetAsync(string endpoint)
    {
        string scope = _configuration["FastApiService:Scope"] ?? throw new InvalidOperationException("Scope configuration is missing");
        string accessToken = await _tokenAcquisition.GetAccessTokenForAppAsync(scope);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(_configuration["FastApiService:BaseUrl"] + "/" + endpoint);

        var responseContent = await response.Content.ReadAsStringAsync();

        return new ApiResponse
        {
            Content = responseContent,
            StatusCode = response.StatusCode
        };
    }

    public async Task<ApiResponse> CallDownstreamFastApiPostAsync(string endpoint, StringContent content)
    {
        string scope = _configuration["FastApiService:Scope"] ?? throw new InvalidOperationException("Scope configuration is missing");
        string accessToken = await _tokenAcquisition.GetAccessTokenForAppAsync(scope);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync(_configuration["FastApiService:BaseUrl"] + "/" + endpoint, content);

        var responseContent = await response.Content.ReadAsStringAsync();

        return new ApiResponse
        {
            Content = responseContent,
            StatusCode = response.StatusCode
        };
    }
}
