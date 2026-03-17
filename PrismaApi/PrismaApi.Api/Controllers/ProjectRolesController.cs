using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectRolesController : PrismaBaseEntityController
{
    private readonly IProjectRoleService _projectRoleService;
    private readonly IUserService _userService;

    public ProjectRolesController(
        IProjectRoleService projectRoleService,
        AppDbContext dbContext,
        IUserService userService

    )
        : base(dbContext)
    {
        _projectRoleService = projectRoleService;
        _userService = userService;
    }

    [HttpGet("project-roles")]
    public async Task<ActionResult<List<ProjectRoleOutgoingDto>>> GetAllProjectRoles()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _projectRoleService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpGet("project-roles/{id:guid}")]
    public async Task<ActionResult<List<ProjectRoleOutgoingDto>>> GetProjectRole(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _projectRoleService.GetAsync(new List<Guid> { id }, user);
        return Ok(result);
    }

    [HttpPut("project-roles")]
    public async Task<ActionResult<List<ProjectRoleOutgoingDto>>> UpdateProjectRoles([FromBody] List<ProjectRoleIncomingDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _projectRoleService.UpdateAsync(dtos, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _projectRoleService.DeleteAsync(new List<Guid> { id }, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _projectRoleService.DeleteAsync(ids, user);
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
