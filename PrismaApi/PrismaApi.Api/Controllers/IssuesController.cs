using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class IssuesController : ControllerBase
{
    private readonly IssueService _issueService;

    public IssuesController(IssueService issueService)
    {
        _issueService = issueService;
    }

    [HttpPost("issues")]
    public async Task<ActionResult<List<IssueOutgoingDto>>> CreateIssues([FromBody] List<IssueIncomingDto> dtos)
    {
        var result = await _issueService.CreateAsync(dtos);
        return Ok(result);
    }

    [HttpGet("issues/{id:guid}")]
    public async Task<ActionResult<IssueOutgoingDto>> GetIssue(Guid id)
    {
        var result = await _issueService.GetAsync(new List<Guid> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("issues")]
    public async Task<ActionResult<List<IssueOutgoingDto>>> GetAllIssues()
    {
        var result = await _issueService.GetAllAsync();
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
        var result = await _issueService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("issues/{id:guid}")]
    public async Task<IActionResult> DeleteIssue(Guid id)
    {
        await _issueService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("issues")]
    public async Task<IActionResult> DeleteIssues([FromQuery] List<Guid> ids)
    {
        await _issueService.DeleteAsync(ids);
        return NoContent();
    }
}
