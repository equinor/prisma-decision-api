using Microsoft.AspNetCore.Mvc;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;
using System.Text.Json;
using System.Text;
using static PrismaApi.Application.Services.FastApiService;
using PrismaApi.Application.Interfaces.Services;

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
        return await _fastApiService.CallDownstreamFastApiPostAsync($"/solvers/project/{projectId}/decision_tree/v2", content);
    }

    [HttpGet("solvers/project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsync([FromRoute] Guid projectId)
    {
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        return await _fastApiService.CallDownstreamFastApiPostAsync($"/solvers/project/{projectId}", content);
    }
}