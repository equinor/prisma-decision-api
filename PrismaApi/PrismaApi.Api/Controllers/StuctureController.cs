using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using Scampi.Domain.Extensions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class StuctureController : ControllerBase
{
    public readonly IFastApiService _fastApiService;
    public StuctureController(IFastApiService fastApiService)
    {
        _fastApiService = fastApiService;
    }

    [HttpGet("structure/{projectId:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetDecisionTreeAsync([FromRoute] Guid projectId)
    {
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/decision_tree/v2");
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("structure/{projectId:guid}/influence_diagram")]
    public async Task<ActionResult<ApiResponseDto>> GetInfluenceDiagramAsync([FromRoute] Guid projectId)
    {
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/influence_diagram");
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("structure/{projectId:guid}/partial_order")]
    public async Task<ActionResult<ApiResponseDto>> GetPartialOrderAsync([FromRoute] Guid projectId)
    {
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/structure/{projectId}/partial_order");
        
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }
}