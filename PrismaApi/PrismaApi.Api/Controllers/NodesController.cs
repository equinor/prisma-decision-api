using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
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
    public async Task<ActionResult<NodeOutgoingDto>> GetNode(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _nodeService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("nodes")]
    public async Task<ActionResult<List<NodeOutgoingDto>>> GetAllNodes()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _nodeService.GetAllAsync(user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _nodeService.UpdateAsync(dtos, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeService.DeleteAsync(new List<Guid> { id }, user);
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
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeService.DeleteAsync(ids, user);
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
