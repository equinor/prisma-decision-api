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
public class SolversController : PrismaBaseController
{
    public readonly IFastApiService _fastApiService;
    public SolversController(IFastApiService fastApiService)
    {
        _fastApiService = fastApiService;
    }

    [HttpGet("solvers/project/{projectId:guid}/decision_tree/v2")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsDecisionTreeAsync([FromRoute] Guid projectId)
    {
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}/decision_tree/v2");
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("solvers/project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsync([FromRoute] Guid projectId)
    {
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}");
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }
}