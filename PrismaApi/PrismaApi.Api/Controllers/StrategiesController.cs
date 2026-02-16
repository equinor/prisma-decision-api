using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class StrategiesController : PrismaBaseEntityController
{
    private readonly StrategyService _strategyService;

    public StrategiesController(StrategyService strategyService, AppDbContext dbContext)
        : base(dbContext)
    {
        _strategyService = strategyService;
    }

    [HttpPost("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> CreateStrategies([FromBody] List<StrategyIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _strategyService.CreateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpPut("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> UpdateStrategies([FromBody] List<StrategyIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _strategyService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _strategyService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("strategies")]
    public async Task<IActionResult> DeleteStrategies([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _strategyService.DeleteAsync(ids);
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
