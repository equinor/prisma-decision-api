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
    public async Task<ActionResult<ValueMetricOutgoingDto>> GetValueMetric(Guid id)
    {
        var result = await _valueMetricService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("value-metrics")]
    public async Task<ActionResult<List<ValueMetricOutgoingDto>>> GetAllValueMetrics()
    {
        var result = await _valueMetricService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("value-metrics")]
    public async Task<ActionResult<List<ValueMetricOutgoingDto>>> UpdateValueMetrics([FromBody] List<ValueMetricIncomingDto> dtos)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _valueMetricService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("value-metrics/{id:guid}")]
    public async Task<IActionResult> DeleteValueMetric(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _valueMetricService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("value-metrics")]
    public async Task<IActionResult> DeleteValueMetrics([FromQuery] List<Guid> ids)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _valueMetricService.DeleteAsync(ids);
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
