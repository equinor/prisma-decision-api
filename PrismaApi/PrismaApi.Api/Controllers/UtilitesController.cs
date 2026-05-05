using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Api.Extensions;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class utilitiesController : PrismaBaseEntityController
{
    private readonly IUtilityService _utilityService;
    private readonly ITableRebuildingService _tableRebuildingService;
    private readonly IUserService _userService;
    public utilitiesController(
        IUtilityService utilityService,
        AppDbContext dbContext,
        ITableRebuildingService tableRebuildingService,
        IUserService userService)
        : base(dbContext)
    {
        _utilityService = utilityService;
        _tableRebuildingService = tableRebuildingService;
        _userService = userService;
    }

    [HttpGet("utilities/{id:guid}")]
    public async Task<ActionResult<UtilityOutgoingDto>> GetUtility(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _utilityService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("utilities")]
    public async Task<ActionResult<List<UtilityOutgoingDto>>> GetAllutilities(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _utilityService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpPut("utilities")]
    public async Task<ActionResult<List<UtilityOutgoingDto>>> Updateutilities([FromBody] List<UtilityIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _utilityService.UpdateAsync(dtos, user, ct);
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

    [HttpDelete("utilities/{id:guid}")]
    public async Task<IActionResult> DeleteUtility(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _utilityService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("utilities")]
    public async Task<IActionResult> Deleteutilities([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _utilityService.DeleteAsync(ids, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpPost("utilities/{id:guid}/table_cleanup")]
    public async Task<IActionResult> CleanupUtilityTableAsync([FromQuery] Guid id, CancellationToken ct = default) 
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        await BeginTransactionAsync(ct);
        try
        {
            // check that the user has access to the utility
            var utility = await _utilityService.GetAsync([id], user, ct) ?? throw new ArgumentException($"Utility {id} not found or User lacks access to the Project");

            await _tableRebuildingService.RemoveExcessDiscreteUtilities(id, ct);
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
