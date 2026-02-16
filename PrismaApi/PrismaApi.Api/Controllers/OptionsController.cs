using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class OptionsController : PrismaBaseEntityController
{
    private readonly OptionService _optionService;

    public OptionsController(OptionService optionService, AppDbContext dbContext)
        : base(dbContext)
    {
        _optionService = optionService;
    }

    [HttpPost("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> CreateOptions([FromBody] List<OptionIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _optionService.CreateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpGet("options/{id:guid}")]
    public async Task<ActionResult<OptionOutgoingDto>> GetOption(Guid id)
    {
        var result = await _optionService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> GetAllOptions()
    {
        var result = await _optionService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> UpdateOptions([FromBody] List<OptionIncomingDto> dtos)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _optionService.UpdateAsync(dtos);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("options/{id:guid}")]
    public async Task<IActionResult> DeleteOption(Guid id)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _optionService.DeleteAsync(new List<Guid> { id });
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("options")]
    public async Task<IActionResult> DeleteOptions([FromQuery] List<Guid> ids)
    {
        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _optionService.DeleteAsync(ids);
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
