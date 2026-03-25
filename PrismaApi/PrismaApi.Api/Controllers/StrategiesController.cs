using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class StrategiesController : PrismaBaseEntityController
{
    private readonly IStrategyService _strategyService;
    private readonly IUserService _userService;

    public StrategiesController(
        IStrategyService strategyService,
        AppDbContext dbContext,
        IUserService userService
    )
        : base(dbContext)
    {
        _strategyService = strategyService;
        _userService = userService;
    }

    [HttpPost("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> CreateStrategies([FromBody] List<StrategyIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _strategyService.CreateAsync(dtos, user);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpPut("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> UpdateStrategies([FromBody] List<StrategyIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _strategyService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("strategies/{id:guid}")]
    public async Task<ActionResult<StrategyOutgoingDto>> GetStrategy(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _strategyService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("strategies")]
    public async Task<ActionResult<List<StrategyOutgoingDto>>> GetAllStrategies(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _strategyService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/strategies")]
    public IActionResult GetStrategiesByProject(Guid projectId, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("strategies/{id:guid}")]
    public async Task<IActionResult> DeleteStrategy(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _strategyService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("strategies")]
    public async Task<IActionResult> DeleteStrategies([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _strategyService.DeleteAsync(ids, user, ct);
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
