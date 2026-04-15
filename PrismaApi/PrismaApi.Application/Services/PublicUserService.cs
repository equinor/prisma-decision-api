using Microsoft.AspNetCore.Http;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using Scampi.Domain.Extensions;

namespace PrismaApi.Application.Services;

public class PublicUserService : BaseUserService
{
    public PublicUserService(IUserRepository userRepository) : base(userRepository)
    {
    }

    protected override async Task<UserOutgoingDto> ResolveUserAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("A user ID is required for public instance.");

        var dto = new UserIncomingDto { Id = userId, Name = "Public User" };
        return (await UserRepository.GetOrAddByIdAsync(dto)).ToOutgoingDto();
    }

    public override async Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context)
    {
        var userName = context.Request.Headers[AppConstants.PublicUsernameHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(userName))
        {
            throw new InvalidOperationException("Username is required");
        }

        var dto = new UserIncomingDto { Id = Guid.NewGuid().ToString(), Name = userName };
        return (await UserRepository.GetOrAddByUserNameAsync(dto)).ToOutgoingDto();
    }

    public override async Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
    {
        string sanitizedQuery = query.SanitizeQuery();
        var users = await UserRepository.GetAllAsync(
            withTracking: false,
            filterPredicate: u => u.Name.Contains(sanitizedQuery));

        return users.ToOutgoingDtos();
    }
}
