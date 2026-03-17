using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _outcomeService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> GetAllOutcomes()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _outcomeService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("outcomes")]
    public async Task<ActionResult<List<OutcomeOutgoingDto>>> UpdateOutcomes([FromBody] List<OutcomeIncomingDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _outcomeService.UpdateAsync(dtos, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _outcomeService.DeleteAsync(new List<Guid> { id }, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _outcomeService.DeleteAsync(ids, user);
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
