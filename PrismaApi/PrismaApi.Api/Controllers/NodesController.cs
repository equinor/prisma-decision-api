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
public class NodesController : PrismaBaseEntityController
{
    private readonly NodeService _nodeService;

    public NodesController(NodeService nodeService, AppDbContext dbContext)
        : base(dbContext)
    {
        _nodeService = nodeService;
    }

    [HttpGet("nodes/{id:guid}")]
    public async Task<ActionResult<NodeOutgoingDto>> GetNode(Guid id)
    {
        var result = await _nodeService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("nodes")]
    public async Task<ActionResult<List<NodeOutgoingDto>>> GetAllNodes()
    {
        var result = await _nodeService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/nodes")]
    public IActionResult GetNodesByProject(Guid projectId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("nodes")]
    public async Task<ActionResult<List<NodeOutgoingDto>>> UpdateNodes([FromBody] List<NodeIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _nodeService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("nodes/{id:guid}")]
    public async Task<IActionResult> DeleteNode(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("nodes")]
    public async Task<IActionResult> DeleteNodes([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeService.DeleteAsync(ids);
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
