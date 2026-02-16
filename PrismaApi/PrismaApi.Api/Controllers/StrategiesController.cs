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
public class StrategiesController : ControllerBase
{
    private readonly StrategyService _strategyService;

    public StrategiesController(StrategyService strategyService)
    {
        _strategyService = strategyService;
    }

    [HttpPost("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> CreateStrategies([FromBody] List<StrategyIncomingDto> dtos)
    {
        var result = await _strategyService.CreateAsync(dtos);
        return Ok(result);
    }

    [HttpPut("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> UpdateStrategies([FromBody] List<StrategyIncomingDto> dtos)
    {
        var result = await _strategyService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpGet("strategies/{id:guid}")]
    public async Task<ActionResult<StrategyOutgoingDto>> GetStrategy(Guid id)
    {
        var result = await _strategyService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> GetAllStrategies()
    {
        var result = await _strategyService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/strategies")]
    public IActionResult GetStrategiesByProject(Guid projectId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("strategies/{id:guid}")]
    public async Task<IActionResult> DeleteStrategy(Guid id)
    {
        await _strategyService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("strategies")]
    public async Task<IActionResult> DeleteStrategies([FromQuery] List<Guid> ids)
    {
        await _strategyService.DeleteAsync(ids);
        return NoContent();
    }
}
