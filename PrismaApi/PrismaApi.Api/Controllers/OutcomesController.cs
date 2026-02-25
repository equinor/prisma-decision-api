using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class OutcomesController : PrismaBaseEntityController
{
    private readonly OutcomeService _outcomeService;
    private readonly TableRebuildingService _tableRebuildingService;

    public OutcomesController(OutcomeService outcomeService, AppDbContext dbContext, TableRebuildingService tableRebuildingService)
        : base(dbContext)
    {
        _outcomeService = outcomeService;
        _tableRebuildingService = tableRebuildingService;
    }

    [HttpPost("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> CreateOutcomes([FromBody] List<OutcomeIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _outcomeService.CreateAsync(dtos);
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _outcomeService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("outcomes/{id:guid}")]
    public async Task<IActionResult> DeleteOutcome(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _outcomeService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("outcomes")]
    public async Task<IActionResult> DeleteOutcomes([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _outcomeService.DeleteAsync(ids);
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
