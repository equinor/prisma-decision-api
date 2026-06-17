using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class RestrictionTablesController : PrismaBaseEntityController
{
    private readonly IRestrictionTableService _restrictionTableService;

    public RestrictionTablesController(
        IRestrictionTableService restrictionTableService,
        AppDbContext dbContext)
        : base(dbContext)
    {
        _restrictionTableService = restrictionTableService;
    }

    [HttpPost("restriction_tables")]
    public async Task<ActionResult<List<RestrictionTableOutgoingDto>>> CreateRestrictionTables([FromBody] List<RestrictionTableIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _restrictionTableService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("restriction_tables/{id:guid}")]
    public async Task<ActionResult<RestrictionTableOutgoingDto>> GetRestrictionTable(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _restrictionTableService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("restriction_tables")]
    public async Task<ActionResult<List<RestrictionTableOutgoingDto>>> GetAllRestrictionTables(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _restrictionTableService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("restriction_tables")]
    public async Task<ActionResult<List<RestrictionTableOutgoingDto>>> UpdateRestrictionTables([FromBody] List<RestrictionTableIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _restrictionTableService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("restriction_tables/{id:guid}")]
    public async Task<IActionResult> DeleteRestrictionTable(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _restrictionTableService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("restriction_tables")]
    public async Task<IActionResult> DeleteRestrictionTables([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _restrictionTableService.DeleteAsync(ids, user, ct);
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
