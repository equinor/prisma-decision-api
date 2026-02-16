using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> CreateProjects([FromBody] List<ProjectCreateDto> dtos)
    {
        var result = await _projectService.CreateAsync(dtos);
        return Ok(result);
    }

    [HttpGet("projects/{id:guid}")]
    public async Task<ActionResult<ProjectOutgoingDto>> GetProject(Guid id)
    {
        var result = await _projectService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> GetAllProjects()
    {
        var result = await _projectService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> UpdateProjects([FromBody] List<ProjectIncomingDto> dtos)
    {
        var result = await _projectService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("projects/{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        await _projectService.DeleteAsync(new List<Guid> { id });
        return NoContent();
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
