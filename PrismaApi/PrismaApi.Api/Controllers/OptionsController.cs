using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class OptionsController : ControllerBase
{
    private readonly OptionService _optionService;

    public OptionsController(OptionService optionService)
    {
        _optionService = optionService;
    }

    [HttpPost("options")]
    public async Task<ActionResult<List<OptionOutgoingDto>>> CreateOptions([FromBody] List<OptionIncomingDto> dtos)
    {
        var result = await _optionService.CreateAsync(dtos);
        return Ok(result);
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
        var result = await _optionService.UpdateAsync(dtos);
        return Ok(result);
    }

    [HttpDelete("options/{id:guid}")]
    public async Task<IActionResult> DeleteOption(Guid id)
    {
        await _optionService.DeleteAsync(new List<Guid> { id });
        return NoContent();
    }

    [HttpDelete("options")]
    public async Task<IActionResult> DeleteOptions([FromQuery] List<Guid> ids)
    {
        await _optionService.DeleteAsync(ids);
        return NoContent();
    }
}
