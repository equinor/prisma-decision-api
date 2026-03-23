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
public class UsersController : PrismaBaseEntityController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService, AppDbContext dbContext)
        : base(dbContext)
    {
        _userService = userService;
    }

    [HttpGet("user/me")]
    public async Task<ActionResult<UserOutgoingDto>> GetMe()
    {
        return Ok(HttpContext.GetLoadedUser());
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserOutgoingDto>>> GetUsers()
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<UserOutgoingDto>> GetUser(string id)
    {
        var result = await _userService.GetAsync(new List<string> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("users/azure-id/{azureId}")]
    public async Task<ActionResult<UserOutgoingDto>> GetUserByAzureId(string azureId)
    {
        var result = await _userService.GetByAzureIdAsync(azureId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("users/search")]
    public async Task<ActionResult<List<UserOutgoingDto>>> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter is required.");

        var result = await _userService.SearchUsersFromGraphAsync(query);
        return Ok(result);
    }

    [HttpGet("auth")]
    public IActionResult AuthPlaceholder()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
