using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Api.Extensions;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class UncertaintiesController : PrismaBaseEntityController
{
    private readonly IUncertaintyService _uncertaintyService;
    private readonly ITableRebuildingService _tableRebuildingService;
    private readonly IUserService _userService;
    public UncertaintiesController(
        IUncertaintyService uncertaintyService,
        AppDbContext dbContext,
        ITableRebuildingService tableRebuildingService,
        IUserService userService)
        : base(dbContext)
    {
        _uncertaintyService = uncertaintyService;
        _tableRebuildingService = tableRebuildingService;
        _userService = userService;
    }

    [HttpGet("uncertainties/{id:guid}")]
    public async Task<ActionResult<UncertaintyOutgoingDto>> GetUncertainty(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _uncertaintyService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound(ct);
    }

    [HttpGet("uncertainties")]
    public async Task<ActionResult<List<UncertaintyOutgoingDto>>> GetAllUncertainties(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _uncertaintyService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("uncertainties")]
    public async Task<ActionResult<List<UncertaintyOutgoingDto>>> UpdateUncertainties([FromBody] List<UncertaintyIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _uncertaintyService.UpdateAsync(dtos, user, ct);
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

    [HttpDelete("uncertainties/{id:guid}")]
    public async Task<IActionResult> DeleteUncertainty(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _uncertaintyService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("uncertainties")]
    public async Task<IActionResult> DeleteUncertainties([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _uncertaintyService.DeleteAsync(ids, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpPost("uncertainties/{id:guid}/remake-probability-table")]
    public async Task<IActionResult> RemakeProbabilityTable(Guid id, CancellationToken ct = default)
    {
        await _tableRebuildingService.RebuildIssuesFromIssueIds([id], ct);
        return Ok(ct);
    }
}
