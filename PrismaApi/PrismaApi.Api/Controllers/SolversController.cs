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
public class SolversController : ControllerBase
{
    public readonly IFastApiService _fastApiService;
    public readonly IProjectService _projectService;
    public SolversController(IFastApiService fastApiService, IProjectService projectService)
    {
        _fastApiService = fastApiService;
        _projectService = projectService;
    }

    [HttpGet("solvers/project/{projectId:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsDecisionTreeAsync([FromRoute] Guid projectId)
    {
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        var fastApiResponse = await _fastApiService.CallDownstreamFastApiPostAsync($"/solvers/project/{projectId}/decision_tree/v2", content);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }
        else
        {
            return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
        }
    }

    [HttpGet("solvers/project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsync([FromRoute] Guid projectId)
    {
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        var fastApiResponse = await _fastApiService.CallDownstreamFastApiPostAsync($"/solvers/project/{projectId}", content);
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