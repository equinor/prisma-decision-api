using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class NodesController : PrismaBaseEntityController
{
    private readonly INodeService _nodeService;
    private readonly IUserService _userService;

    public NodesController(
        INodeService nodeService,
        AppDbContext dbContext,
        IUserService userService)
        : base(dbContext)
    {
        _nodeService = nodeService;
        _userService = userService;
    }

    [HttpGet("nodes/{id:guid}")]
    public async Task<ActionResult<NodeOutgoingDto>> GetNode(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _nodeService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("nodes")]
    public async Task<ActionResult<List<NodeOutgoingDto>>> GetAllNodes(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _nodeService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/nodes")]
    public IActionResult GetNodesByProject(Guid projectId, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("nodes")]
    public async Task<ActionResult<List<NodeOutgoingDto>>> UpdateNodes([FromBody] List<NodeIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _nodeService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("nodes/{id:guid}")]
    public async Task<IActionResult> DeleteNode(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _nodeService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("nodes")]
    public async Task<IActionResult> DeleteNodes([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _nodeService.DeleteAsync(ids, user, ct);
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
