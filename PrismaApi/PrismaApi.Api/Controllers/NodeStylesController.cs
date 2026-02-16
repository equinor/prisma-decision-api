using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class NodeStylesController : PrismaBaseEntityController
{
    private readonly NodeStyleService _nodeStyleService;

    public NodeStylesController(NodeStyleService nodeStyleService, AppDbContext dbContext)
        : base(dbContext)
    {
        _nodeStyleService = nodeStyleService;
    }

    [HttpGet("node-styles/{id:guid}")]
    public async Task<ActionResult<NodeStyleOutgoingDto>> GetNodeStyle(Guid id)
    {
        var result = await _nodeStyleService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("node-styles")]
    public async Task<ActionResult<List<NodeStyleOutgoingDto>>> GetAllNodeStyles()
    {
        var result = await _nodeStyleService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("node-styles")]
    public async Task<ActionResult<List<NodeStyleOutgoingDto>>> UpdateNodeStyles([FromBody] List<NodeStyleIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _nodeStyleService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("node-styles/{id:guid}")]
    public async Task<IActionResult> DeleteNodeStyle(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeStyleService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("node-styles")]
    public async Task<IActionResult> DeleteNodeStyles([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeStyleService.DeleteAsync(ids);
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
