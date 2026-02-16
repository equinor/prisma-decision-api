using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Infrastructure;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectRolesController : PrismaBaseEntityController
{
    private readonly ProjectRoleService _projectRoleService;

    public ProjectRolesController(ProjectRoleService projectRoleService, AppDbContext dbContext)
        : base(dbContext)
    {
        _projectRoleService = projectRoleService;
    }

    [HttpGet("project-roles")]
    public async Task<ActionResult<List<ProjectRoleOutgoingDto>>> GetAllProjectRoles()
    {
        var result = await _projectRoleService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("project-roles/{id:guid}")]
    public async Task<ActionResult<List<ProjectRoleOutgoingDto>>> GetProjectRole(Guid id)
    {
        var result = await _projectRoleService.GetAsync(new List<Guid> { id });
        return Ok(result);
    }

    [HttpPut("project-roles")]
    public async Task<ActionResult<List<ProjectRoleOutgoingDto>>> UpdateProjectRoles([FromBody] List<ProjectRoleIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _projectRoleService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("project-roles/{projectId:guid}/{id:guid}")]
    public async Task<IActionResult> DeleteProjectRole(Guid projectId, Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _projectRoleService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("project-roles/{projectId:guid}")]
    public async Task<IActionResult> DeleteProjectRoles(Guid projectId, [FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _projectRoleService.DeleteAsync(ids);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }
}
