using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

[ApiController]
[Route("")]
public class UsersController : PrismaBaseEntityController
{
    private readonly UserService _userService;

    public UsersController(UserService userService, AppDbContext dbContext)
        : base(dbContext)
    {
        _userService = userService;
    }

    [HttpGet("user/me")]
    public async Task<ActionResult<UserOutgoingDto>> GetMe()
    {
        var mockUser = new UserIncomingDto
        {
            AzureId = "28652cc8-c5ed-43c7-a6b0-c2a4ce3d7185",
            Name = "Einar Salomonsen (Bouvet Norge AS)"
        };

        return Ok(await _userService.GetOrCreateUserByAzureIdAsync(mockUser));
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserOutgoingDto>>> GetUsers()
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<UserOutgoingDto>> GetUser(int id)
    {
        var result = await _userService.GetAsync(new List<int> { id });
        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [HttpGet("users/azure-id/{azureId}")]
    public async Task<ActionResult<UserOutgoingDto>> GetUserByAzureId(string azureId)
    {
        var result = await _userService.GetByAzureIdAsync(azureId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("auth")]
    public IActionResult AuthPlaceholder()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
