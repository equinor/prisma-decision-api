using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class OutcomesController : PrismaBaseEntityController
{
    private readonly IOutcomeService _outcomeService;
    private readonly ITableRebuildingService _tableRebuildingService;
    private readonly IUserService _userService;

    public OutcomesController(
        IOutcomeService outcomeService,
        AppDbContext dbContext,
        ITableRebuildingService tableRebuildingService,
        IUserService userService)
        : base(dbContext)
    {
        _outcomeService = outcomeService;
        _tableRebuildingService = tableRebuildingService;
        _userService = userService;
    }

    [HttpPost("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> CreateOutcomes([FromBody] List<OutcomeIncomingDto> dtos, CancellationToken ct = default)
    {
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _outcomeService.CreateAsync(dtos, ct);
            await _tableRebuildingService.RebuildTablesAsync(ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("outcomes/{id:guid}")]
    public async Task<ActionResult<OutcomeOutgoingDto>> GetOutcome(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _outcomeService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound(ct);
    }

    [HttpGet("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> GetAllOutcomes(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _outcomeService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> UpdateOutcomes([FromBody] List<OutcomeIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _outcomeService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("outcomes/{id:guid}")]
    public async Task<IActionResult> DeleteOutcome(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _outcomeService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("outcomes")]
    public async Task<IActionResult> DeleteOutcomes([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _outcomeService.DeleteAsync(ids, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
