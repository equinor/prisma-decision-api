using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using Scampi.Domain.Extensions;
using System.Net;
using System.Text;
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
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsDecisionTreeAsync([FromRoute] Guid projectId)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}/decision_tree/v2", user);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }

    [HttpGet("solvers/project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponseDto>> GetSolutionAsync([FromRoute] Guid projectId)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var fastApiResponse = await _fastApiService.SendInfluenceDiagramToFastApiAsync(projectId, $"/solvers/project/{projectId}", user);
        if (fastApiResponse.StatusCode == HttpStatusCode.OK)
        {
            return Ok(!string.IsNullOrEmpty(fastApiResponse.Content) ? fastApiResponse.Content.SanitizeString() : null);
        }

        return StatusCode((int)fastApiResponse.StatusCode, fastApiResponse.Content);
    }
}
