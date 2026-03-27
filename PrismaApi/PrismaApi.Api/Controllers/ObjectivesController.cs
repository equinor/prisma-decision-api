using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[Route("")]
public class ObjectivesController : PrismaBaseEntityController
{
    private readonly IObjectiveService _objectiveService;
    private readonly IUserService _userService;

    public ObjectivesController(
        IObjectiveService objectiveService, 
        AppDbContext dbContext,
        IUserService userService
    )
        : base(dbContext)
    {
        _objectiveService = objectiveService;
        _userService = userService;
    }

    [HttpPost("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> CreateObjectives([FromBody] List<ObjectiveIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _objectiveService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("objectives/{id:guid}")]
    public async Task<ActionResult<ObjectiveOutgoingDto>> GetObjective(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _objectiveService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> GetAllObjectives(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _objectiveService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/objectives")]
    public IActionResult GetObjectivesByProject(Guid projectId, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("objectives")]
    public async Task<ActionResult<List<ObjectiveOutgoingDto>>> UpdateObjectives([FromBody] List<ObjectiveIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _objectiveService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("objectives/{id:guid}")]
    public async Task<IActionResult> DeleteObjective(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _objectiveService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("objectives")]
    public async Task<IActionResult> DeleteObjectives([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _objectiveService.DeleteAsync(ids, user, ct);
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
