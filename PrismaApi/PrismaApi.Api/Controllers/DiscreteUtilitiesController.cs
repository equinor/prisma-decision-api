using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

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
    public async Task<ActionResult<List<DiscreteUtilityDto>>> CreateDiscreteUtilities([FromBody] List<DiscreteUtilityDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _discreteUtilityService.CreateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpGet("discrete_utilities/{id:guid}")]
    public async Task<ActionResult<DiscreteUtilityDto>> GetDiscreteUtility(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _discreteUtilityService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("discrete_utilities")]
    public async Task<ActionResult<List<DiscreteUtilityDto>>> GetAllDiscreteUtilities()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _discreteUtilityService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpPut("discrete_utilities")]
    public async Task<ActionResult<List<DiscreteUtilityDto>>> UpdateDiscreteUtilities([FromBody] List<DiscreteUtilityDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _discreteUtilityService.UpdateAsync(dtos, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("discrete_utilities/{id:guid}")]
    public async Task<IActionResult> DeleteDiscreteUtility(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _discreteUtilityService.DeleteAsync(new List<Guid> { id }, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("discrete_utilities")]
    public async Task<IActionResult> DeleteDiscreteUtilities([FromQuery] List<Guid> ids)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _discreteUtilityService.DeleteAsync(ids, user);
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
