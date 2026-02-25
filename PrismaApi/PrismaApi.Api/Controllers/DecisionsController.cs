using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Infrastructure;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class DecisionsController : PrismaBaseEntityController
{
    private readonly DecisionService _decisionService;
    private readonly TableRebuildingService _tableRebuildingService;

    public DecisionsController(DecisionService decisionService, AppDbContext dbContext, TableRebuildingService tableRebuildingService)
        : base(dbContext)
    {
        _decisionService = decisionService;
        _tableRebuildingService = tableRebuildingService;
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _decisionService.UpdateAsync(dtos);
            await _tableRebuildingService.RebuildTablesAsync();
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("decisions/{id:guid}")]
    public async Task<IActionResult> DeleteDecision(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _decisionService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("decisions")]
    public async Task<IActionResult> DeleteDecisions([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _decisionService.DeleteAsync(ids);
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
