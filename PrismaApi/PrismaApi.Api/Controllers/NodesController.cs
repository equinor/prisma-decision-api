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
public class NodesController : ControllerBase
{
    private readonly NodeService _nodeService;

    public NodesController(NodeService nodeService)
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
        var result = await _nodeService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("nodes/{id:guid}")]
    public async Task<IActionResult> DeleteNode(Guid id)
    {
        await _nodeService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("nodes")]
    public async Task<IActionResult> DeleteNodes([FromQuery] List<Guid> ids)
    {
        await _nodeService.DeleteAsync(ids);
        return NoContent();
    }
}
