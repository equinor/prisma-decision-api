using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class RestrictionEntriesController : PrismaBaseEntityController
{
    private readonly IRestrictionEntryService _restrictionEntryService;

    public RestrictionEntriesController(
        IRestrictionEntryService restrictionEntryService,
        AppDbContext dbContext)
        : base(dbContext)
    {
        _restrictionEntryService = restrictionEntryService;
    }

    [HttpPost("restriction_entries")]
    public async Task<ActionResult<List<RestrictionEntryOutgoingDto>>> CreateRestrictionEntries([FromBody] List<RestrictionEntryIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _restrictionEntryService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("restriction_entries/{id:guid}")]
    public async Task<ActionResult<RestrictionEntryOutgoingDto>> GetRestrictionEntry(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _restrictionEntryService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("restriction_entries")]
    public async Task<ActionResult<List<RestrictionEntryOutgoingDto>>> GetAllRestrictionEntries(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _restrictionEntryService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("restriction_entries")]
    public async Task<ActionResult<List<RestrictionEntryOutgoingDto>>> UpdateRestrictionEntries([FromBody] List<RestrictionEntryIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _restrictionEntryService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("restriction_entries/{id:guid}")]
    public async Task<IActionResult> DeleteRestrictionEntry(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _restrictionEntryService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("restriction_entries")]
    public async Task<IActionResult> DeleteRestrictionEntries([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _restrictionEntryService.DeleteAsync(ids, user, ct);
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
