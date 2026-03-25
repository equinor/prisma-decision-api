using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Extensions;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class DiscreteUtilitiesController : PrismaBaseEntityController
{
    private readonly IDiscreteUtilityService _discreteUtilityService;
    private readonly IUserService _userService;

    public DiscreteUtilitiesController(
        IDiscreteUtilityService discreteUtilityService,
        AppDbContext dbContext,
        IUserService userService)
        : base(dbContext)
    {
        _discreteUtilityService = discreteUtilityService;
        _userService = userService;
    }

    [HttpPost("discrete_utilities")]
    public async Task<ActionResult<List<DiscreteUtilityDto>>> CreateDiscreteUtilities([FromBody] List<DiscreteUtilityDto> dtos, CancellationToken ct = default)
    {
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _discreteUtilityService.CreateAsync(dtos);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("discrete_utilities/{id:guid}")]
    public async Task<ActionResult<DiscreteUtilityDto>> GetDiscreteUtility(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _discreteUtilityService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("discrete_utilities")]
    public async Task<ActionResult<List<DiscreteUtilityDto>>> GetAllDiscreteUtilities(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _discreteUtilityService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("discrete_utilities")]
    public async Task<ActionResult<List<DiscreteUtilityDto>>> UpdateDiscreteUtilities([FromBody] List<DiscreteUtilityDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _discreteUtilityService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("discrete_utilities/{id:guid}")]
    public async Task<IActionResult> DeleteDiscreteUtility(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _discreteUtilityService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("discrete_utilities")]
    public async Task<IActionResult> DeleteDiscreteUtilities([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _discreteUtilityService.DeleteAsync(ids, user, ct);
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
