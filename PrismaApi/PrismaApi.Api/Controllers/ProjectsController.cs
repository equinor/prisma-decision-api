using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Infrastructure;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ProjectsController : PrismaBaseEntityController
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService, AppDbContext dbContext)
        : base(dbContext)
    {
        _projectService = projectService;
    }

    [HttpPost("projects")]
    public async Task<ActionResult<List<ProjectOutgoingDto>>> CreateProjects([FromBody] List<ProjectCreateDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _projectService.CreateAsync(dtos);
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _projectService.UpdateAsync(dtos);
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _projectService.DeleteAsync(new List<Guid> { id });
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
