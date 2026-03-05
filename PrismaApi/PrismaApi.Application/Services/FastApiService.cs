using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Net;
using System.Net.Http.Headers;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;

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

    public async Task<ApiResponseDto> CallDownstreamFastApiGetAsync(string endpoint)
    {
        string scope = _configuration["FastApiService:Scope"] ?? throw new InvalidOperationException("Scope configuration is missing");
        string accessToken = await _tokenAcquisition.GetAccessTokenForAppAsync(scope);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var normalizedEndpoint = endpoint.TrimStart('/');
        var response = await _httpClient.GetAsync(_configuration["FastApiService:BaseUrl"] + "/" + normalizedEndpoint);

        var responseContent = await response.Content.ReadAsStringAsync();

        return new ApiResponseDto
        {
            Content = responseContent,
            StatusCode = response.StatusCode
        };
    }

    public async Task<ApiResponseDto> CallDownstreamFastApiPostAsync(string endpoint, StringContent content)
    {
        string scope = _configuration["FastApiService:Scope"] ?? throw new InvalidOperationException("Scope configuration is missing");
        string accessToken = await _tokenAcquisition.GetAccessTokenForAppAsync(scope);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        try
        {

            var normalizedEndpoint = endpoint.TrimStart('/');
            var response = await _httpClient.PostAsync(_configuration["FastApiService:BaseUrl"] + "/" + normalizedEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return new ApiResponseDto
            {
                Content = responseContent,
                StatusCode = response.StatusCode
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


    }
}
