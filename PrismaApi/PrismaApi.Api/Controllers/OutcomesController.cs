using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class OutcomesController : ControllerBase
{
    private readonly OutcomeService _outcomeService;

    public OutcomesController(OutcomeService outcomeService)
    {
        _outcomeService = outcomeService;
    }

    [HttpPost("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> CreateOutcomes([FromBody] List<OutcomeIncomingDto> dtos)
    {
        var result = await _outcomeService.CreateAsync(dtos);
        return Ok(result);
    }

    [HttpGet("outcomes/{id:guid}")]
    public async Task<ActionResult<OutcomeOutgoingDto>> GetOutcome(Guid id)
    {
        var result = await _outcomeService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> GetAllOutcomes()
    {
        var result = await _outcomeService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> UpdateOutcomes([FromBody] List<OutcomeIncomingDto> dtos)
    {
        var result = await _outcomeService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("outcomes/{id:guid}")]
    public async Task<IActionResult> DeleteOutcome(Guid id)
    {
        await _outcomeService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("outcomes")]
    public async Task<IActionResult> DeleteOutcomes([FromQuery] List<Guid> ids)
    {
        await _outcomeService.DeleteAsync(ids);
        return NoContent();
    }
}
