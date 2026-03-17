using Microsoft.AspNetCore.Mvc;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;
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
    public async Task<ActionResult<List<ProjectOutgoingDto>>> CreateProjects([FromBody] List<ProjectCreateDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _projectService.CreateAsync(dtos, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpGet("projects/{id:guid}")]
    public async Task<ActionResult<ProjectOutgoingDto>> GetProject(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _projectService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> GetAllProjects()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        var result = await _projectService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> UpdateProjects([FromBody] List<ProjectIncomingDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _projectService.UpdateAsync(dtos, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("projects/{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _projectService.DeleteAsync(new List<Guid> { id }, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
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
