using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Infrastructure;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;

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
    public async Task<ActionResult<DecisionOutgoingDto>> GetDecision(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _decisionService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("decisions")]
    public async Task<ActionResult<List<DecisionOutgoingDto>>> GetAllDecisions()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _decisionService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("decisions")]
    public async Task<ActionResult<List<DecisionOutgoingDto>>> UpdateDecisions([FromBody] List<DecisionIncomingDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _decisionService.UpdateAsync(dtos, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _decisionService.DeleteAsync(new List<Guid> { id }, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _decisionService.DeleteAsync(ids, user);
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
