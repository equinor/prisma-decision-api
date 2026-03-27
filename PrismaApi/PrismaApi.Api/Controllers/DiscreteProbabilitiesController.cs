using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class DiscreteProbabilitiesController : PrismaBaseEntityController
{
    private readonly IDiscreteProbabilityService _discreteProbabilityService;
    private readonly IUserService _userService;

    public DiscreteProbabilitiesController(
        IDiscreteProbabilityService discreteProbabilityService,
        AppDbContext dbContext,
        IUserService userService)
        : base(dbContext)
    {
        _discreteProbabilityService = discreteProbabilityService;
        _userService = userService;
    }

    [HttpPost("discrete_probabilities")]
    public async Task<ActionResult<List<DiscreteProbabilityDto>>> CreateDiscreteProbabilities([FromBody] List<DiscreteProbabilityDto> dtos, CancellationToken ct = default)
    {
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _discreteProbabilityService.CreateAsync(dtos);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("discrete_probabilities/{id:guid}")]
    public async Task<ActionResult<DiscreteProbabilityDto>> GetDiscreteProbability(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _discreteProbabilityService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("discrete_probabilities")]
    public async Task<ActionResult<List<DiscreteProbabilityDto>>> GetAllDiscreteProbabilities(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _discreteProbabilityService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("discrete_probabilities")]
    public async Task<ActionResult<List<DiscreteProbabilityDto>>> UpdateDiscreteProbabilities([FromBody] List<DiscreteProbabilityDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _discreteProbabilityService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("discrete_probabilities/{id:guid}")]
    public async Task<IActionResult> DeleteDiscreteProbability(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _discreteProbabilityService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("discrete_probabilities")]
    public async Task<IActionResult> DeleteDiscreteProbabilities([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _discreteProbabilityService.DeleteAsync(ids, user, ct);
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
