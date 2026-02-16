using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class DecisionsController : ControllerBase
{
    private readonly DecisionService _decisionService;

    public DecisionsController(DecisionService decisionService)
    {
        _decisionService = decisionService;
    }

    [HttpGet("decisions/{id:guid}")]
    public async Task<ActionResult<DecisionOutgoingDto>> GetDecision(Guid id)
    {
        var result = await _decisionService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("decisions")]
    public async Task<ActionResult<List<DecisionOutgoingDto>>> GetAllDecisions()
    {
        var result = await _decisionService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("decisions")]
    public async Task<ActionResult<List<DecisionOutgoingDto>>> UpdateDecisions([FromBody] List<DecisionIncomingDto> dtos)
    {
        var result = await _decisionService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("decisions/{id:guid}")]
    public async Task<IActionResult> DeleteDecision(Guid id)
    {
        await _decisionService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("decisions")]
    public async Task<IActionResult> DeleteDecisions([FromQuery] List<Guid> ids)
    {
        await _decisionService.DeleteAsync(ids);
        return NoContent();
    }
}
