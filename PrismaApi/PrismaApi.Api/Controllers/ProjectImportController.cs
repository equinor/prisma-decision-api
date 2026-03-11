using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Configuration.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using System.Net;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = SecurityPolicy.UserRoleRequired)]
public class ProjectImportController : PrismaBaseController
{
    private readonly IProjectImportService _projectImportService;
    private readonly IUserService _userService;

    public ProjectImportController(
        IProjectImportService projectImportService,
        IUserService userService
    )
    {
        _projectImportService = projectImportService;
        _userService = userService;
    }


    [HttpPost("import")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> ImportProjectsAsync(
        List<ProjectImportDto> importDtos,
        CancellationToken cancellationToken = default
    )
    {
        if (importDtos == null || importDtos.Count == 0)
        {
            return BadRequest("No projects provided for import");
        }

        try
        {
            // Get current user from claims
            var user = await _userService.GetOrCreateUserFromGraphMeAsync(
                GetUserCacheKeyFromClaims()
            );

            // Import projects using duplication logic
            var createdProjects = await _projectImportService.ImportFromJsonWithDuplicationLogicAsync(
                importDtos,
                user,
                cancellationToken
            );

            return Ok(createdProjects);
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "An error occurred during project import", error = ex.Message });
        }
    }
}
