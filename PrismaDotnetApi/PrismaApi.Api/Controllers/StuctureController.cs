using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Extensions;
using System.Net;
using PrismaApi.Api.Extensions;

namespace PrismaApi.Api.Controllers;

[ApiController]

public class StuctureController : PrismaBaseController
{
    private readonly IFastApiService _fastApiService;
    private readonly IUserService _userService;
    private readonly IInfluenceDiagramService _influenceDiagramService;
    public StuctureController(IFastApiService fastApiService, IUserService userService, IInfluenceDiagramService influenceDiagramService)
    {
        _fastApiService = fastApiService;
        _userService = userService;
        _influenceDiagramService = influenceDiagramService;
    }

    [HttpGet("structure/{projectId:guid}/test_total_restrictions")]
    public async Task<ActionResult<ICollection<DiscreteUtilityDto>>> TestTotalRestrictionsAsync([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        var influenceDiagram = await _influenceDiagramService.GetInfluenceDiagramAsync(projectId, user, ct);
        if (influenceDiagram is null)
        {
            return NotFound();
        }

        influenceDiagram.ApplyTotalRestrictions();

        return Ok(influenceDiagram.discreteUtilities);
    }

    [HttpGet("structure/{projectId:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetDecisionTreeAsync([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/decision_tree/v2", user, true, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("structure/{projectId:guid}/influence_diagram")]
    public async Task<ActionResult<ApiResponseDto>> GetInfluenceDiagramAsync([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/influence_diagram", user, true, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("structure/{projectId:guid}/partial_order")]
    public async Task<ActionResult<ApiResponseDto>> GetPartialOrderAsync([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/partial_order", user, true, ct);

        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("structure/{projectId:guid}/decision_tree/v3")]
    public async Task<ActionResult<ApiResponseDto>> GetDecisionTreeV3Async([FromRoute] Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/decision_tree/v3", user, true, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpPost("structure/{projectId:guid}/partial_decision_tree/v3")]
    public async Task<ActionResult<ApiResponseDto>> GetPartialDecisionTreeV3Async([FromRoute] Guid projectId, [FromBody] List<List<Guid>> paths, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var fastApiResponse = await _fastApiService.SendPartialInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/partial_decision_tree/v3", paths, user, ct);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeLogString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }
}
