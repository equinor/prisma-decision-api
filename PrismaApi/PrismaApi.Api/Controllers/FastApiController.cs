using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;
using System.Text.Json;
using System.Text;
using static PrismaApi.Application.Services.FastApiService;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class FastApiController : ControllerBase
{
    public readonly FastApiService _fastApiService;
    public readonly ProjectService _projectService;
    public FastApiController(FastApiService fastApiService, ProjectService projectService)
    {
        _fastApiService = fastApiService;
        _projectService = projectService;
    }

    [HttpGet("structure/{id:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponse>> GetDecisionTreeAsync([FromRoute] Guid projectId)
    {
        // get content from issue and edge dtos from the database
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        return await _fastApiService.CallDownstreamFastApiPostAsync($"/structure/{projectId}/decision_tree/v2", new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json"));
    }
}