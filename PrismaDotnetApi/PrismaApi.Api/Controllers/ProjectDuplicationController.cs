using Microsoft.AspNetCore.Mvc;
using PrismaApi.Api.Controllers;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Api.Extensions;
using PrismaApi.Infrastructure.Context;

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
    public async Task<ActionResult<ProjectOutgoingDto>> DuplicateProject(Guid id, CancellationToken ct = default)
    {
        UserOutgoingDto user = HttpContext.GetLoadedUser();

        await BeginTransactionAsync(ct);
        try
        {
            var result = await _duplicationService.DuplicateAsync(id, user, ct);
            await CommitTransactionAsync(ct);
            return Ok(result);
        }
        catch
        {
            await RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
