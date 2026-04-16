using Microsoft.AspNetCore.Http;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using Scampi.Domain.Extensions;

namespace PrismaApi.Application.Services;

public class PublicUserService : IUserProvider
{
    private readonly IUserRepository _userRepository;

    public PublicUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserOutgoingDto> ResolveUserFromContextAsync(HttpContext context)
    {
        var userName = context.Request.Headers[AppConstants.PublicUsernameHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(userName))
        {
            throw new InvalidOperationException("Username is required");
        }

        var dto = new UserIncomingDto { Id = Guid.NewGuid().ToString(), Name = userName };
        return (await _userRepository.GetOrAddByUserNameAsync(dto)).ToOutgoingDto();
    }

    public async Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
    {
        string sanitizedQuery = query.SanitizeQuery();
        var users = await _userRepository.GetAllAsync(
            withTracking: false,
            filterPredicate: u => u.Name.Contains(sanitizedQuery));

        return users.ToOutgoingDtos();
    }
}
