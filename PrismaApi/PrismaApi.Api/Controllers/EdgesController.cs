using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Domain.Dtos;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class EdgesController : PrismaBaseEntityController
{
    private readonly IEdgeService _edgeService;
    private readonly ITableRebuildingService _tableRebuildingService;
    private readonly IUserService _userService;

    public EdgesController(
        IEdgeService edgeService,
        AppDbContext dbContext,
        ITableRebuildingService tableRebuildingService,
        IUserService userService)
        : base(dbContext)
    {
        _edgeService = edgeService;
        _tableRebuildingService = tableRebuildingService;
        _userService = userService;
    }

    [HttpPost("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> CreateEdges([FromBody] List<EdgeIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _edgeService.CreateAsync(dtos);
            await _tableRebuildingService.RebuildTablesAsync();
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
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _edgeService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> GetAllEdges()
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _edgeService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> UpdateEdges([FromBody] List<EdgeIncomingDto> dtos)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _edgeService.UpdateAsync(dtos, user);
            await _tableRebuildingService.RebuildTablesAsync();
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
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _edgeService.DeleteAsync(new List<Guid> { id }, user);
            await _tableRebuildingService.RebuildTablesAsync();
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
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _edgeService.DeleteAsync(ids, user);
            await _tableRebuildingService.RebuildTablesAsync();
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
