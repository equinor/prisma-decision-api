using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using PrismaApi.Application.Repositories;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrismaApi.Api.Controllers;

[Route("")]
public class ObjectivesController : PrismaBaseEntityController
{
    private readonly ObjectiveService _objectiveService;

    public ObjectivesController(
        ObjectiveService objectiveService, 
        GraphServiceClient graphServiceClient,
        UserRepository userRepository,
        AppDbContext dbContext
    )
        : base(dbContext, graphServiceClient, userRepository)
    {
        _objectiveService = objectiveService;
    }

    [HttpPost("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> CreateObjectives([FromBody] List<ObjectiveIncomingDto> dtos)
    {
        UserOutgoingDto user = await GetOrCreateUserFromGraphMeAsync();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _objectiveService.CreateAsync(dtos, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpGet("objectives/{id:guid}")]
    public async Task<ActionResult<ObjectiveOutgoingDto>> GetObjective(Guid id)
    {
        var result = await _objectiveService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> GetAllObjectives()
    {
        var result = await _objectiveService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/objectives")]
    public IActionResult GetObjectivesByProject(Guid projectId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> UpdateObjectives([FromBody] List<ObjectiveIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _objectiveService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("objectives/{id:guid}")]
    public async Task<IActionResult> DeleteObjective(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _objectiveService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("objectives")]
    public async Task<IActionResult> DeleteObjectives([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _objectiveService.DeleteAsync(ids);
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
