using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class EdgesController : ControllerBase
{
    private readonly EdgeService _edgeService;

    public EdgesController(EdgeService edgeService)
    {
        _edgeService = edgeService;
    }

    [HttpPost("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> CreateEdges([FromBody] List<EdgeIncomingDto> dtos)
    {
        var result = await _edgeService.CreateAsync(dtos);
        return Ok(result);
    }

    [HttpGet("edges/{id:guid}")]
    public async Task<ActionResult<EdgeOutgoingDto>> GetEdge(Guid id)
    {
        var result = await _edgeService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> GetAllEdges()
    {
        var result = await _edgeService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> UpdateEdges([FromBody] List<EdgeIncomingDto> dtos)
    {
        var result = await _edgeService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("edges/{id:guid}")]
    public async Task<IActionResult> DeleteEdge(Guid id)
    {
        await _edgeService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("edges")]
    public async Task<IActionResult> DeleteEdges([FromQuery] List<Guid> ids)
    {
        await _edgeService.DeleteAsync(ids);
        return NoContent();
    }
}
