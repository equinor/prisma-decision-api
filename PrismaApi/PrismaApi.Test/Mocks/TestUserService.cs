using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Test.Mocks;

public class TestUserService : BaseUserService
{
    public TestUserService(IUserRepository userRepository) : base(userRepository)
    {
    }

    protected override async Task<UserOutgoingDto> ResolveUserAsync(string? userId)
    {
        var id = userId ?? Guid.NewGuid().ToString();
        var user = await UserRepository.GetOrAddByIdAsync(new UserIncomingDto
        {
            Id = id,
            Name = "Test User"
        });
        return user.ToOutgoingDto();
    }

    public override Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
    {
        return Task.FromResult(new List<UserOutgoingDto>());
    }

    public override async Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context)
    {
        var oid = context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.Oid)?.Value
            ?? context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value;

        if (string.IsNullOrEmpty(oid))
        {
            throw new InvalidOperationException("No Id found in Claims");
        }

        var name = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Test User";
        var user = await UserRepository.GetOrAddByIdAsync(new UserIncomingDto
        {
            Id = oid,
            Name = name
        });

        return user.ToOutgoingDto();
    }
}
