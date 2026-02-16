using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectRolesController : ControllerBase
{
    private readonly ProjectRoleService _projectRoleService;

    public ProjectRolesController(ProjectRoleService projectRoleService)
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
        var result = await _projectRoleService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("project-roles/{projectId:guid}/{id:guid}")]
    public async Task<IActionResult> DeleteProjectRole(Guid projectId, Guid id)
    {
        await _projectRoleService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("project-roles/{projectId:guid}")]
    public async Task<IActionResult> DeleteProjectRoles(Guid projectId, [FromQuery] List<Guid> ids)
    {
        await _projectRoleService.DeleteAsync(ids);
        return NoContent();
    }
}
