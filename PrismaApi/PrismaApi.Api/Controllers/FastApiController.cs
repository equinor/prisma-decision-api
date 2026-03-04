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
public class FastApiController : ControllerBase
{
    public readonly IFastApiService _fastApiService;
    public readonly IProjectService _projectService;
    public FastApiController(IFastApiService fastApiService, IProjectService projectService)
    {
        _fastApiService = fastApiService;
        _projectService = projectService;
    }

    [HttpGet("structure/{id:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetDecisionTreeAsync([FromRoute] Guid projectId)
    {
        // get content from issue and edge dtos from the database
        var influanceDiagram = await _projectService.GetInfluanceDiagramAsync(projectId);
        var content = new StringContent(JsonSerializer.Serialize(influanceDiagram), Encoding.UTF8, "application/json");
        return await _fastApiService.CallDownstreamFastApiPostAsync($"/structure/{projectId}/decision_tree/v2", content);
    }
}