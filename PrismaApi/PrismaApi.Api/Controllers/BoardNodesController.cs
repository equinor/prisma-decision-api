using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Api.Controllers;

[Route("")]
public class BoardNodesController : PrismaBaseEntityController
{
    private readonly IBoardNodeService _boardNodeService;
    
    public BoardNodesController(
        IBoardNodeService boardNodeService, 
        AppDbContext dbContext
    )
        : base(dbContext)
    {
        _boardNodeService = boardNodeService;
    }

    [HttpPost("board_nodes")]
    public async Task<ActionResult<List<BoardNodeOutgoingDto>>> CreateBoardNodes([FromBody] List<BoardNodeIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _boardNodeService.CreateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpGet("board_nodes/{id:guid}")]
    public async Task<ActionResult<BoardNodeOutgoingDto>> GetBoardNode(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _boardNodeService.GetAsync(new List<Guid> { id }, user, ct);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("board_nodes")]
    public async Task<ActionResult<List<BoardNodeOutgoingDto>>> GetAllBoardNodes(CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();
        var result = await _boardNodeService.GetAllAsync(user, ct);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/boardNodes")]
    public IActionResult GetBoardNodesByProject(Guid projectId, CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("board_nodes")]
    public async Task<ActionResult<List<BoardNodeOutgoingDto>>> UpdateBoardNodes([FromBody] List<BoardNodeIncomingDto> dtos, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _boardNodeService.UpdateAsync(dtos, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("board_nodes/{id:guid}")]
    public async Task<IActionResult> DeleteBoardNode(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _boardNodeService.DeleteAsync(new List<Guid> { id }, user, ct);
            await CommitTransactionAsync(ct);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpDelete("board_nodes")]
    public async Task<IActionResult> DeleteBoardNodes([FromQuery] List<Guid> ids, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            await _boardNodeService.DeleteAsync(ids, user, ct);
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
