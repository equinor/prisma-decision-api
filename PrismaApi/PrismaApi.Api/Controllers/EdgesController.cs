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
    public async Task<ActionResult<List<EdgeOutgoingDto>>> CreateEdges([FromBody] List<EdgeIncomingDto> dtos, CancellationToken ct = default)
    {
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _edgeService.CreateAsync(dtos, ct);
            await _tableRebuildingService.RebuildTablesAsync(ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("edges/{id:guid}")]
    public async Task<ActionResult<EdgeOutgoingDto>> GetEdge(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _edgeService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound(ct);
    }

    [HttpGet("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> GetAllEdges(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _edgeService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("edges")]
    public async Task<ActionResult<List<EdgeOutgoingDto>>> UpdateEdges([FromBody] List<EdgeIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _edgeService.UpdateAsync(dtos, user, ct);
            await _tableRebuildingService.RebuildTablesAsync(ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("edges/{id:guid}")]
    public async Task<IActionResult> DeleteEdge(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _edgeService.DeleteAsync(new List<Guid> { id }, user, ct);
            await _tableRebuildingService.RebuildTablesAsync(ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("edges")]
    public async Task<IActionResult> DeleteEdges([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _edgeService.DeleteAsync(ids, user, ct);
            await _tableRebuildingService.RebuildTablesAsync(ct);
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
