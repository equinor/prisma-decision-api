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
public class ObjectivesController : ControllerBase
{
    private readonly ObjectiveService _objectiveService;

    public ObjectivesController(ObjectiveService objectiveService)
    {
        _objectiveService = objectiveService;
    }

    [HttpPost("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> CreateObjectives([FromBody] List<ObjectiveIncomingDto> dtos)
    {
        var result = await _objectiveService.CreateAsync(dtos);
        return Ok(result);
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
        var result = await _objectiveService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("objectives/{id:guid}")]
    public async Task<IActionResult> DeleteObjective(Guid id)
    {
        await _objectiveService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("objectives")]
    public async Task<IActionResult> DeleteObjectives([FromQuery] List<Guid> ids)
    {
        await _objectiveService.DeleteAsync(ids);
        return NoContent();
    }
}
