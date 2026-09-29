using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[Route("")]
public class StakeholderMatrixController : PrismaBaseEntityController
{
    private readonly IStakeholderMatrixService _stakeholderMatrixService;
    private readonly IUserService _userService;

    public StakeholderMatrixController(
        IStakeholderMatrixService stakeholderMatrixService,
        AppDbContext dbContext,
        IUserService userService
    )
        : base(dbContext)
    {
        _stakeholderMatrixService = stakeholderMatrixService;
        _userService = userService;
    }

    [HttpPost("stakeholder-matrix")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> CreateObjectives([FromBody] List<StakeholderMatrixIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _stakeholderMatrixService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("stakeholder-matrix/{id:guid}")]
    public async Task<ActionResult<StakeholderMatrixDto>> GetObjective(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _stakeholderMatrixService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("stakeholder-matrix")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> GetAllObjectives(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _stakeholderMatrixService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/stakeholder-matrix")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> GetStakeholderMatrixByProject(Guid projectId, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _stakeholderMatrixService.GetByProjectAsync(projectId, user, ct);
        return Ok(result);
    }

    [HttpPut("stakeholder-matrix")]
    public async Task<ActionResult<List<StakeholderMatrixDto>>> UpdateObjectives([FromBody] List<StakeholderMatrixIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _stakeholderMatrixService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("stakeholder-matrix/{id:guid}")]
    public async Task<IActionResult> DeleteObjective(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _stakeholderMatrixService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("stakeholder-matrix")]
    public async Task<IActionResult> DeleteObjectives([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _stakeholderMatrixService.DeleteAsync(ids, user, ct);
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
