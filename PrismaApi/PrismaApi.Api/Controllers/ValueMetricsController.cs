using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class ValueMetricsController : ControllerBase
{
    private readonly ValueMetricService _valueMetricService;

    public ValueMetricsController(ValueMetricService valueMetricService)
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
        var result = await _valueMetricService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("value-metrics/{id:guid}")]
    public async Task<IActionResult> DeleteValueMetric(Guid id)
    {
        await _valueMetricService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("value-metrics")]
    public async Task<IActionResult> DeleteValueMetrics([FromQuery] List<Guid> ids)
    {
        await _valueMetricService.DeleteAsync(ids);
        return NoContent();
    }
}
