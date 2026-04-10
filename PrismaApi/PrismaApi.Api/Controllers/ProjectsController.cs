using Microsoft.AspNetCore.Mvc;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectsController : PrismaBaseEntityController
{
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;

    public ProjectsController(
        IProjectService projectService,
        AppDbContext dbContext,
        IUserService userService

    )
        : base(dbContext)
    {
        _projectService = projectService;
        _userService = userService;
    }

    [HttpPost("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> CreateProjects([FromBody] List<ProjectCreateDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        bool createDefaultRole = true;
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _projectService.CreateAsync(dtos, createDefaultRole, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("projects/{id:guid}")]
    public async Task<ActionResult<ProjectOutgoingDto>> GetProject(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _projectService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> GetAllProjects(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        var result = await _projectService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> UpdateProjects([FromBody] List<ProjectIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _projectService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
    }

    [HttpDelete("projects/{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        await BeginTransactionAsync(ct);
        try
        {
            await _projectService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
    }

    [HttpGet("projects-populated/{id:guid}")]
    public IActionResult GetPopulatedProject(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpGet("projects-populated")]
    public IActionResult GetAllPopulatedProjects()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
