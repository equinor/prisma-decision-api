using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class OptionsController : PrismaBaseEntityController
{
    private readonly IOptionService _optionService;
    private readonly ITableRebuildingService _tableRebuildingService;
    private readonly IUserService _userService;

    public OptionsController(
        IOptionService optionService,
        AppDbContext dbContext,
        ITableRebuildingService tableRebuildingService,
        IUserService userService)
        : base(dbContext)
    {
        _optionService = optionService;
        _tableRebuildingService = tableRebuildingService;
        _userService = userService;
    }

    [HttpPost("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> CreateOptions([FromBody] List<OptionIncomingDto> dtos, CancellationToken ct = default)
    {
        await BeginTransactionAsync(ct);
        try
        {
            var result = await _optionService.CreateAsync(dtos, ct);
            await _tableRebuildingService.RebuildTablesAsync(ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("options/{id:guid}")]
    public async Task<ActionResult<OptionOutgoingDto>> GetOption(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _optionService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound(ct);
    }

    [HttpGet("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> GetAllOptions(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _optionService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> UpdateOptions([FromBody] List<OptionIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _optionService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("options/{id:guid}")]
    public async Task<IActionResult> DeleteOption(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _optionService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("options")]
    public async Task<IActionResult> DeleteOptions([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _optionService.DeleteAsync(ids, user, ct);
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
