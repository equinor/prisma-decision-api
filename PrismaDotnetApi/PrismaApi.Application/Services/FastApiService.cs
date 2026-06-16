using Microsoft.Extensions.Configuration;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using System.Text;
using System.Text.Json;

namespace PrismaApi.Application.Services;

public class FastApiService : IFastApiService
{
    private readonly HttpClient _httpClient;
    private readonly IProjectService _projectService;
    private readonly IConfiguration _configuration;
    public FastApiService(HttpClient httpClient, IProjectService projectService, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(180);
        _projectService = projectService;
        _configuration = configuration;
    }

    public async Task<ApiResponseDto> CallDownstreamFastApiGetAsync(string endpoint, CancellationToken ct = default)
    {
        var normalizedEndpoint = endpoint.TrimStart('/');
        var response = await _httpClient.GetAsync(_configuration["FastApiService:BaseUrl"] + "/" + normalizedEndpoint, ct);

        var responseContent = await response.Content.ReadAsStringAsync(ct);

        return new ApiResponseDto
        {
            Content = responseContent,
            StatusCode = response.StatusCode
        };
    }

    public async Task<ApiResponseDto> CallDownstreamFastApiPostAsync(string endpoint, StringContent content, CancellationToken ct = default)
    {
        try
        {
            var normalizedEndpoint = endpoint.TrimStart('/');
            var response = await _httpClient.PostAsync(_configuration["FastApiService:BaseUrl"] + "/" + normalizedEndpoint, content, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

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

    public async Task<ApiResponseDto> SendInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, UserOutgoingDto user, CancellationToken ct = default)
    {
        var influanceDiagram = await _projectService.GetInfluenceDiagramAsync(projectId, user, ct);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }

    public async Task<ApiResponseDto> SendPartialInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, List<List<Guid>> paths, UserOutgoingDto user, CancellationToken ct = default)
    {
        var influanceDiagram = await _projectService.GetInfluenceDiagramAsync(projectId, user, ct);
        var payload = new
        {
            issues = influanceDiagram.issues,
            edges = influanceDiagram.edges,
            paths
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }

    public async Task<ApiResponseDto> SendInfluenceDiagramWithEvidenceToFastApiAsync(Guid projectId, string endpoint, List<EvidenceRequestDto> data, UserOutgoingDto user, CancellationToken ct = default)
    {
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId, user, ct);
        var payload = new
        {
            issues = influanceDiagram.issues,
            edges = influanceDiagram.edges,
            evidence = data,
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }
}
