using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrismaApi.Api.Extensions;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ValueMetricsController : PrismaBaseEntityController
{
    private readonly IValueMetricService _valueMetricService;

    public ValueMetricsController(IValueMetricService valueMetricService, AppDbContext dbContext)
        : base(dbContext)
    {
        _valueMetricService = valueMetricService;
    }

    [HttpGet("value-metrics/{id:guid}")]
    public async Task<ActionResult<ValueMetricOutgoingDto>> GetValueMetric(Guid id, CancellationToken ct = default)
    {
        var result = await _valueMetricService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("value-metrics")]
    public async Task<ActionResult<List<ValueMetricOutgoingDto>>> GetAllValueMetrics(CancellationToken ct = default)
    {
        var result = await _valueMetricService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("value-metrics")]
    public async Task<ActionResult<List<ValueMetricOutgoingDto>>> UpdateValueMetrics([FromBody] List<ValueMetricIncomingDto> dtos, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _valueMetricService.UpdateAsync(dtos);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("value-metrics/{id:guid}")]
    public async Task<IActionResult> DeleteValueMetric(Guid id, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
        await BeginTransactionAsync(ct);
        try
        {
            await _valueMetricService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("value-metrics")]
    public async Task<IActionResult> DeleteValueMetrics([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
        await BeginTransactionAsync(ct);
        try
        {
            await _valueMetricService.DeleteAsync(ids);
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
