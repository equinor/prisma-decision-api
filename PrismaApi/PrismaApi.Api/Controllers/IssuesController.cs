using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class IssuesController : PrismaBaseEntityController
{
    private readonly IIssueService _issueService;
    private readonly IUserService _userService;
    private readonly ITableRebuildingService _tableRebuildingService;

    public IssuesController(
        IIssueService issueService,
        IUserService userService,
        ITableRebuildingService tableRebuildingService,
        AppDbContext dbContext
    )
        : base(dbContext)
    {
        _issueService = issueService;
        _userService = userService;
        _tableRebuildingService = tableRebuildingService;
    }

    [HttpPost("issues")]
    public async Task<ActionResult<List<IssueOutgoingDto>>> CreateIssues([FromBody] List<IssueIncomingDto> dtos, CancellationToken cancellationToken = default)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _issueService.CreateAsync(dtos, user);
            await CommitTransactionAsync(cancellationToken);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    [HttpGet("issues/{id:guid}")]
    public async Task<ActionResult<IssueOutgoingDto>> GetIssue(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _issueService.GetAsync(new List<Guid> { id }, user);
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("issues")]
    public async Task<ActionResult<List<IssueOutgoingDto>>> GetAllIssues()
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
        var result = await _issueService.GetAllAsync(user);
        return Ok(result);
    }

    [HttpGet("projects/{projectId:guid}/issues")]
    public IActionResult GetIssuesByProject(Guid projectId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPut("issues")]
    public async Task<ActionResult<List<IssueOutgoingDto>>> UpdateIssues([FromBody] List<IssueIncomingDto> dtos)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _issueService.UpdateAsync(dtos, user);
            await _tableRebuildingService.RebuildTablesAsync();
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("issues/{id:guid}")]
    public async Task<IActionResult> DeleteIssue(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _issueService.DeleteAsync(new List<Guid> { id }, user);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return NoContent();
        }
        catch
        {
            await RollbackTransactionAsync(HttpContext.RequestAborted);
            throw;
        }
    }

    [HttpDelete("issues")]
    public async Task<IActionResult> DeleteIssues([FromQuery] List<Guid> ids)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            await _issueService.DeleteAsync(ids, user);
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
