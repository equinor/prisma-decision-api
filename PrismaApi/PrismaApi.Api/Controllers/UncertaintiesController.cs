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
    public async Task<ActionResult<UncertaintyOutgoingDto>> GetUncertainty(Guid id)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _uncertaintyService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("uncertainties")]
    public async Task<ActionResult<List<UncertaintyOutgoingDto>>> GetAllUncertainties()
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _uncertaintyService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("uncertainties")]
    public async Task<ActionResult<List<UncertaintyOutgoingDto>>> UpdateUncertainties([FromBody] List<UncertaintyIncomingDto> dtos)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _uncertaintyService.UpdateAsync(dtos, user);
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

    [HttpDelete("uncertainties/{id:guid}")]
    public async Task<IActionResult> DeleteUncertainty(Guid id)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _uncertaintyService.DeleteAsync(new List<Guid> { id }, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("uncertainties")]
    public async Task<IActionResult> DeleteUncertainties([FromQuery] List<Guid> ids)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _uncertaintyService.DeleteAsync(ids, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpPost("uncertainties/{id:guid}/remake-probability-table")]
    public async Task<IActionResult> RemakeProbabilityTable(Guid id)
    {
        await _tableRebuildingService.RebuildIssuesFromIssueIds([id]);
        return Ok();
    }
}
