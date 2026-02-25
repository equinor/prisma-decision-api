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
public class UncertaintiesController : PrismaBaseEntityController
{
    private readonly UncertaintyService _uncertaintyService;
    private readonly TableRebuildingService _tableRebuildingService;
    public UncertaintiesController(UncertaintyService uncertaintyService, AppDbContext dbContext, TableRebuildingService tableRebuildingService)
        : base(dbContext)
    {
        _uncertaintyService = uncertaintyService;
        _tableRebuildingService = tableRebuildingService;
    }

    [HttpGet("uncertainties/{id:guid}")]
    public async Task<ActionResult<UncertaintyOutgoingDto>> GetUncertainty(Guid id)
    {
        var result = await _uncertaintyService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("uncertainties")]
    public async Task<ActionResult<List<UncertaintyOutgoingDto>>> GetAllUncertainties()
    {
        var result = await _uncertaintyService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("uncertainties")]
    public async Task<ActionResult<List<UncertaintyOutgoingDto>>> UpdateUncertainties([FromBody] List<UncertaintyIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _uncertaintyService.UpdateAsync(dtos);
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _uncertaintyService.DeleteAsync(new List<Guid> { id });
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _uncertaintyService.DeleteAsync(ids);
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
