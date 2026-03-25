using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Configuration.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Api.Extensions;
using System.Net;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectImportController : PrismaBaseEntityController
{
    private readonly IProjectImportService _projectImportService;
    private readonly IUserService _userService;

    public ProjectImportController(
        IProjectImportService projectImportService,
        AppDbContext dbContext,
        IUserService userService
    ) : base(dbContext)
    {
        _projectImportService = projectImportService;
        _userService = userService;
    }


    [HttpPost("import")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> ImportProjectsAsync(
        List<ProjectImportDto> importDtos,
        CancellationToken ct = default
    )
    {
        if (importDtos == null || importDtos.Count == 0)
        {
            return BadRequest("No projects provided for import");
        }



        // Get current user from claims
        var user = HttpContext.GetLoadedUser();
        await BeginTransactionAsync(ct);
        try
        {
            var createdProjects = await _projectImportService.ImportFromJsonWithDuplicationLogicAsync(
                importDtos,
                user,
                ct
            );
            await CommitTransactionAsync(ct);
            return Ok(createdProjects);
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(CancellationToken.None);
            return StatusCode((int)HttpStatusCode.InternalServerError,
                           new { message = "An error occurred during project import", error = ex.Message });
        }
    }
}
