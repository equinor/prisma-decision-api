using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Api.Extensions;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class DecisionsController : PrismaBaseEntityController
{
    private readonly IDecisionService _decisionService;
    private readonly ITableRebuildingService _tableRebuildingService;
    private readonly IUserService _userService;

    public DecisionsController(
        IDecisionService decisionService,
        AppDbContext dbContext,
        ITableRebuildingService tableRebuildingService,
        IUserService userService)
        : base(dbContext)
    {
        _decisionService = decisionService;
        _tableRebuildingService = tableRebuildingService;
        _userService = userService;
    }

    [HttpGet("decisions/{id:guid}")]
    public async Task<ActionResult<DecisionOutgoingDto>> GetDecision(Guid id, CancellationToken ct = default)
    {
        var result = await _decisionService.GetAsync(new List<Guid> { id }, HttpContext.GetLoadedUser(), ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("decisions")]
    public async Task<ActionResult<List<DecisionOutgoingDto>>> GetAllDecisions(CancellationToken ct = default)
    {
        var result = await _decisionService.GetAllAsync(HttpContext.GetLoadedUser(), ct);
        return Ok(result);
    }

    [HttpPut("decisions")]
    public async Task<ActionResult<List<DecisionOutgoingDto>>> UpdateDecisions([FromBody] List<DecisionIncomingDto> dtos, CancellationToken ct = default)
    {
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _decisionService.UpdateAsync(dtos, HttpContext.GetLoadedUser(), ct);
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

    [HttpDelete("decisions/{id:guid}")]
    public async Task<IActionResult> DeleteDecision(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _decisionService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("decisions")]
    public async Task<IActionResult> DeleteDecisions([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _decisionService.DeleteAsync(ids, user, ct);
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
