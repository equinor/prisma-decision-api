using Microsoft.Extensions.Configuration;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Extensions;
using System.Text;
using System.Text.Json;

namespace PrismaApi.Application.Services;

public class FastApiService : IFastApiService
{
    private readonly HttpClient _httpClient;
    private readonly IInfluenceDiagramService _influenceDiagramService;
    private readonly IConfiguration _configuration;
    public FastApiService(HttpClient httpClient, IInfluenceDiagramService influenceDiagramService, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(180);
        _influenceDiagramService = influenceDiagramService;
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

    public async Task<ApiResponseDto> SendInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, UserOutgoingDto user, bool restrictInfluenceDiagram, CancellationToken ct = default)
    {
        var influenceDiagram = restrictInfluenceDiagram
            ? await GetRestrictedInfluenceDiagramWithMarginsAsync(projectId, user, ct)
            : await _influenceDiagramService.GetInfluenceDiagramAsync(projectId, user, ct);
        
        var payload = new
        {
            issues = influenceDiagram.issues,
            edges = influenceDiagram.edges,
            discrete_probabilities = influenceDiagram.discreteProbabilities,
            discrete_utilities = influenceDiagram.discreteUtilities,
            restriction_tables = influenceDiagram.restrictionTables,
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }


    public async Task<ApiResponseDto> SendPartialInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, List<List<Guid>> paths, UserOutgoingDto user, CancellationToken ct = default)
    {
        var influenceDiagram = await GetRestrictedInfluenceDiagramWithMarginsAsync(projectId, user, ct);

        var payload = new
        {
            issues = influenceDiagram.issues,
            edges = influenceDiagram.edges,
            discrete_probabilities = influenceDiagram.discreteProbabilities,
            discrete_utilities = influenceDiagram.discreteUtilities,
            restriction_tables = influenceDiagram.restrictionTables,
            paths
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }

    public async Task<ApiResponseDto> SendInfluenceDiagramWithEvidenceToFastApiAsync(Guid projectId, string endpoint, List<EvidenceRequestDto> data, UserOutgoingDto user, CancellationToken ct = default)
    {
        var influenceDiagram = await GetRestrictedInfluenceDiagramWithMarginsAsync(projectId, user, ct);

        var payload = new
        {
            issues = influenceDiagram.issues,
            edges = influenceDiagram.edges,
            discrete_probabilities = influenceDiagram.discreteProbabilities,
            discrete_utilities = influenceDiagram.discreteUtilities,
            restriction_tables = influenceDiagram.restrictionTables,
            evidence = data,
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }
    public async Task<ApiResponseDto> SendInfluenceDiagramPolicyTableToFastApiAsync(Guid projectId, string endpoint, EvidenceRequestDto? evidence, UserOutgoingDto user, CancellationToken ct = default)
    {
        var influenceDiagram = await GetRestrictedInfluenceDiagramWithMarginsAsync(projectId, user, ct);
        var payload = new
        {
            issues = influenceDiagram.issues,
            edges = influenceDiagram.edges,
            discrete_probabilities = influenceDiagram.discreteProbabilities,
            discrete_utilities = influenceDiagram.discreteUtilities,
            restriction_tables = influenceDiagram.restrictionTables,
            evidence,
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        return await CallDownstreamFastApiPostAsync(endpoint, content, ct);
    }

    public async Task AddMarginsToInfluenceDiagramAsync(InfluenceDiagramDto influenceDiagram, CancellationToken ct = default)
    {
        var payload = new
        {
            issues = influenceDiagram.issues,
            edges = influenceDiagram.edges,
            discrete_probabilities = influenceDiagram.discreteProbabilities,
            discrete_utilities = influenceDiagram.discreteUtilities,
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await CallDownstreamFastApiPostAsync($"/solvers/project/{influenceDiagram.projectId}/margins", content, ct);

        if ((int)response.StatusCode is < 200 or >= 300)
        {
            throw new HttpRequestException(
                $"FastAPI margin calculation failed with status {(int)response.StatusCode}: {response.Content}",
                null,
                response.StatusCode);
        }

        influenceDiagram.margins = string.IsNullOrWhiteSpace(response.Content)
            ? []
            : JsonSerializer.Deserialize<Dictionary<Guid, List<MarginTableRowDto>>>(response.Content) ?? [];
    }

    private async Task<InfluenceDiagramDto> GetRestrictedInfluenceDiagramWithMarginsAsync(
        Guid projectId,
        UserOutgoingDto user,
        CancellationToken ct)
    {
        var influenceDiagram = (await _influenceDiagramService.GetInfluenceDiagramAsync(projectId, user, ct)).DeepClone();
        if (influenceDiagram.RequiresMarginsForRestrictions())
        {
            await AddMarginsToInfluenceDiagramAsync(influenceDiagram, ct);
        }
        influenceDiagram.ApplyRestrictions();
        return influenceDiagram;
    }

    public List<PolicyTableOutgoingDto> ParsePolicyTableResponse(string? content)
    {
        List<PolicyTableFromFastApiDto> response = [];
        if (!string.IsNullOrWhiteSpace(content))
        {
            response = JsonSerializer.Deserialize<List<PolicyTableFromFastApiDto>>(content) ?? [];
        }

        return response
            .Select(row => new PolicyTableOutgoingDto
            {
                DecisionId = Guid.Parse(row.DecisionId),
                ParentStateIds = row.States.Select(Guid.Parse).ToList(),
                OptionId = Guid.Parse(row.OptionId),
                Value = row.Value
            })
            .ToList();
    }
}
