using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[Route("")]
public class BoardSheetsController : PrismaBaseEntityController
{
    private readonly IBoardSheetService _boardSheetService;

    public BoardSheetsController(
        IBoardSheetService boardSheetService,
        AppDbContext dbContext
    )
        : base(dbContext)
    {
        _boardSheetService = boardSheetService;
    }

    [HttpPost("board_sheets")]
    public async Task<ActionResult<List<BoardSheetOutgoingDto>>> CreateBoardSheets([FromBody] List<BoardSheetIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _boardSheetService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("board_sheets/{id:guid}")]
    public async Task<ActionResult<BoardSheetOutgoingDto>> GetBoardSheet(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _boardSheetService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("board_sheets")]
    public async Task<ActionResult<List<BoardSheetOutgoingDto>>> GetAllBoardSheets(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _boardSheetService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/boardSheets")]
    public IActionResult GetBoardSheetsByProject(Guid projectId, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("board_sheets")]
    public async Task<ActionResult<List<BoardSheetOutgoingDto>>> UpdateBoardSheets([FromBody] List<BoardSheetIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _boardSheetService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("board_sheets/{id:guid}")]
    public async Task<IActionResult> DeleteBoardSheet(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _boardSheetService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("board_sheets")]
    public async Task<IActionResult> DeleteBoardSheets([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _boardSheetService.DeleteAsync(ids, user, ct);
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
