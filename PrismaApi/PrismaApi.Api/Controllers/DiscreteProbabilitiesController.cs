using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class DiscreteProbabilitiesController : PrismaBaseEntityController
{
    private readonly IDiscreteProbabilityService _discreteProbabilityService;

    public DiscreteProbabilitiesController(IDiscreteProbabilityService discreteProbabilityService, AppDbContext dbContext)
        : base(dbContext)
    {
        _discreteProbabilityService = discreteProbabilityService;
    }

    [HttpPost("discrete_probabilities")]
    public async Task<ActionResult<List<DiscreteProbabilityDto>>> CreateDiscreteProbabilities([FromBody] List<DiscreteProbabilityDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _discreteProbabilityService.CreateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpGet("discrete_probabilities/{id:guid}")]
    public async Task<ActionResult<DiscreteProbabilityDto>> GetDiscreteProbability(Guid id)
    {
        var result = await _discreteProbabilityService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("discrete_probabilities")]
    public async Task<ActionResult<List<DiscreteProbabilityDto>>> GetAllDiscreteProbabilities()
    {
        var result = await _discreteProbabilityService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("discrete_probabilities")]
    public async Task<ActionResult<List<DiscreteProbabilityDto>>> UpdateDiscreteProbabilities([FromBody] List<DiscreteProbabilityDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _discreteProbabilityService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("discrete_probabilities/{id:guid}")]
    public async Task<IActionResult> DeleteDiscreteProbability(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _discreteProbabilityService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("discrete_probabilities")]
    public async Task<IActionResult> DeleteDiscreteProbabilities([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _discreteProbabilityService.DeleteAsync(ids);
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
