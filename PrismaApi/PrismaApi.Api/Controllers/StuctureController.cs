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
    public readonly IProjectService _projectService;
    public StuctureController(IFastApiService fastApiService, IProjectService projectService)
    {
        _fastApiService = fastApiService;
        _projectService = projectService;
    }

    [HttpGet("structure/{projectId:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetDecisionTreeAsync([FromRoute] Guid projectId)
    {
        // get content from issue and edge dtos from the database
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        var fastApiResponse = await _fastApiService.CallDownstreamFastApiPostAsync($"/structure/{projectId}/decision_tree/v2", content);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }
        else
        {
            return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
        }
    }

    [HttpGet("structure/{projectId:guid}/influence_diagram")]
    public async Task<ActionResult<ApiResponseDto>> GetInfluenceDiagramAsync([FromRoute] Guid projectId)
    {
        // get content from issue and edge dtos from the database
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        var fastApiResponse = await _fastApiService.CallDownstreamFastApiPostAsync($"/structure/{projectId}/influence_diagram", content);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }
        else
        {
            return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
        }
    }

    [HttpGet("structure/{projectId:guid}/partial_order")]
    public async Task<ActionResult<ApiResponseDto>> GetPartialOrderAsync([FromRoute] Guid projectId)
    {
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        var fastApiResponse = await _fastApiService.CallDownstreamFastApiPostAsync($"/structure/{projectId}/partial_order", content);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }
        else
        {
            return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
        }
    }
}