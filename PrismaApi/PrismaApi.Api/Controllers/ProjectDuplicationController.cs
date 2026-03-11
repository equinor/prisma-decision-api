using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Controllers;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

[ApiController]
[Route("")]
public class ProjectDuplicationController : PrismaBaseEntityController
{
    private readonly IProjectDuplicationService _duplicationService;
    private readonly IUserService _userService;

    public ProjectDuplicationController(
        AppDbContext dbContext,
        IProjectDuplicationService duplicationService,
        IUserService userService) : base(dbContext)
    {
        _duplicationService = duplicationService;
        _userService = userService;
    }

    [HttpPost("projects/{id:guid}/duplicate")]
    public async Task<ActionResult<ProjectOutgoingDto>> DuplicateProject(Guid id)
    {
        UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());

        await BeginTransactionAsync(HttpContext.RequestAborted);
        try
        {
            var result = await _duplicationService.DuplicateAsync(id, user, HttpContext.RequestAborted);
            await CommitTransactionAsync(HttpContext.RequestAborted);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
