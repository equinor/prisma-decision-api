using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class UncertaintiesController : ControllerBase
{
    private readonly UncertaintyService _uncertaintyService;

    public UncertaintiesController(UncertaintyService uncertaintyService)
    {
        _uncertaintyService = uncertaintyService;
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
        var result = await _uncertaintyService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("uncertainties/{id:guid}")]
    public async Task<IActionResult> DeleteUncertainty(Guid id)
    {
        await _uncertaintyService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("uncertainties")]
    public async Task<IActionResult> DeleteUncertainties([FromQuery] List<Guid> ids)
    {
        await _uncertaintyService.DeleteAsync(ids);
        return NoContent();
    }

    [HttpPost("uncertainties/{id:guid}/remake-probability-table")]
    public IActionResult RemakeProbabilityTable(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
