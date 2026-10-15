using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[Route("")]
public class StakeholderMatrixEntryController : PrismaBaseEntityController
{
    private readonly IStakeholderMatrixEntryService _stakeholderMatrixEntryService;
    private readonly IUserService _userService;

    public StakeholderMatrixEntryController(
        IStakeholderMatrixEntryService stakeholderMatrixEntryService,
        AppDbContext dbContext,
        IUserService userService
    )
        : base(dbContext)
    {
        _stakeholderMatrixEntryService = stakeholderMatrixEntryService;
        _userService = userService;
    }

    [HttpPost("stakeholder-matrix-entry")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> CreateObjectives([FromBody] List<StakeholderMatrixIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _stakeholderMatrixEntryService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("stakeholder-matrix-entry/{id:guid}")]
    public async Task<ActionResult<StakeholderMatrixDto>> GetObjective(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _stakeholderMatrixEntryService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("stakeholder-matrix-entry")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> GetAllObjectives(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _stakeholderMatrixEntryService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/stakeholder-matrix-entry")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> GetStakeholderMatrixByProject(Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _stakeholderMatrixEntryService.GetByProjectAsync(projectId, user, ct);
        return Ok(result);
    }

    [HttpPut("stakeholder-matrix-entry")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> UpdateObjectives([FromBody] List<StakeholderMatrixIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _stakeholderMatrixEntryService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("stakeholder-matrix-entry/{id:guid}")]
    public async Task<IActionResult> DeleteObjective(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _stakeholderMatrixEntryService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("stakeholder-matrix-entry")]
    public async Task<IActionResult> DeleteObjectives([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _stakeholderMatrixEntryService.DeleteAsync(ids, user, ct);
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
