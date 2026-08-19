using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Extensions;
using System.Net;
using PrismaApi.Api.Extensions;
using System.Text.Json;

namespace PrismaApi.Api.Controllers;

[ApiController]

public class SolversController : PrismaBaseController
{
    private readonly IFastApiService _fastApiService;
    private readonly IUserService _userService;
    public SolversController(IFastApiService fastApiService, IUserService userService)
    {
        _fastApiService = fastApiService;
        _userService = userService;
    }

    [HttpGet("solvers/project/{projectId:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsDecisionTreeAsync([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}/decision_tree/v2", user, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpPost("solvers/project/{projectId:guid}/partial_decision_tree/v3")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsDecisionTreeV3Async([FromRoute] Guid projectId, [FromBody] List<List<Guid>> paths, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendPartialInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}/partial_decision_tree/v3", paths, user, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("solvers/project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsync([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}", user, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpPost("solvers/project/{projectId:guid}/with_evidence")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionWithEvidenceAsync([FromRoute] Guid projectId, [FromBody] List<EvidenceRequestDto> evidence, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramWithEvidenceToFastApiAsync(projectId, $"/solvers/project/{projectId}/with_evidence", evidence, user, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpPost("solvers/project/{projectId:guid}/policy_table")]
    public async Task<ActionResult<List<PolicyTableOutgoingDto>>> GetPolicyTableAsync([FromRoute] Guid projectId, [FromBody] EvidenceRequestDto? evidence = null, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramPolicyTableToFastApiAsync(projectId, $"/solvers/project/{projectId}/policy_table", evidence, user, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            Dictionary<string, List<PolicyTableStatesOutgoingDto>> response = [];
            if (!string.IsNullOrWhiteSpace(fastApiResponse.Content))
            {
                response = JsonSerializer.Deserialize<Dictionary<string, List<PolicyTableStatesOutgoingDto>>>(
                    fastApiResponse.Content
                ) ?? [];
            }

            var result = response
                .Select(kvp => new PolicyTableOutgoingDto
                {
                    DecisionId = kvp.Key,
                    Rows = kvp.Value
                })
                .ToList();

            return Ok(result);
        }
        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }
}
