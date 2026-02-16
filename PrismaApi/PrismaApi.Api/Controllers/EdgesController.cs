using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Infrastructure;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class EdgesController : PrismaBaseEntityController
{
    private readonly EdgeService _edgeService;

    public EdgesController(EdgeService edgeService, AppDbContext dbContext)
        : base(dbContext)
    {
        _edgeService = edgeService;
    }

    [HttpPost("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> CreateEdges([FromBody] List<EdgeIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _edgeService.CreateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
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
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _edgeService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("edges/{id:guid}")]
    public async Task<IActionResult> DeleteEdge(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _edgeService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("edges")]
    public async Task<IActionResult> DeleteEdges([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _edgeService.DeleteAsync(ids);
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
