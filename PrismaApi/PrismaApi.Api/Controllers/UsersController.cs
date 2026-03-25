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
    public async Task<ActionResult<UserOutgoingDto>> GetMe(CancellationToken ct = default)
    {
        return Ok(HttpContext.GetLoadedUser());
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserOutgoingDto>>> GetUsers(CancellationToken ct = default)
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }


    [HttpPost("users/userIds")]
    public async Task<ActionResult<List<UserOutgoingDto>>> GetUsersByIds([FromBody] List<string> ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest("At least one user ID is required.");

        var result = await _userService.GetByIdsAsync(ids);
        return result.Count > 0 ? Ok(result) : NotFound();
    }

    [HttpGet("users/search")]
    public async Task<ActionResult<List<UserOutgoingDto>>> SearchUsers([FromQuery] string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter is required.");

        var result = await _userService.SearchUsersFromGraphAsync(query);
        return result.Count > 0 ? Ok(result) : NotFound();
    }

    [HttpGet("auth")]
    public IActionResult AuthPlaceholder(CancellationToken ct = default)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
