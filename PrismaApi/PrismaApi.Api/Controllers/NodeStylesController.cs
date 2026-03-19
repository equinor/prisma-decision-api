using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class NodeStylesController : PrismaBaseEntityController
{
    private readonly INodeStyleService _nodeStyleService;
    private readonly IUserService _userService;

    public NodeStylesController(
        INodeStyleService nodeStyleService,
        AppDbContext dbContext,
        IUserService userService)
        : base(dbContext)
    {
        _nodeStyleService = nodeStyleService;
        _userService = userService;
    }

    [HttpGet("node-styles/{id:guid}")]
    public async Task<ActionResult<NodeStyleOutgoingDto>> GetNodeStyle(Guid id)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _nodeStyleService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("node-styles")]
    public async Task<ActionResult<List<NodeStyleOutgoingDto>>> GetAllNodeStyles()
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _nodeStyleService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("node-styles")]
    public async Task<ActionResult<List<NodeStyleOutgoingDto>>> UpdateNodeStyles([FromBody] List<NodeStyleIncomingDto> dtos)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _nodeStyleService.UpdateAsync(dtos, user);
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
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeStyleService.DeleteAsync(new List<Guid> { id }, user);
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
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _nodeStyleService.DeleteAsync(ids, user);
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
