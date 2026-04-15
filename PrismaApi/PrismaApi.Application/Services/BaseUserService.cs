using Microsoft.AspNetCore.Http;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public abstract class BaseUserService : IBaseUserService
{
    protected readonly IUserRepository UserRepository;

    protected BaseUserService(IUserRepository userRepository)
    {
        UserRepository = userRepository;
    }

    public async Task<List<UserOutgoingDto>> GetAllAsync()
    {
        var users = await UserRepository.GetAllAsync(withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<List<UserOutgoingDto>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var users = await UserRepository.GetByIdsAsync(ids, withTracking: false);
        return users.ToOutgoingDtos();
    }

    protected abstract Task<UserOutgoingDto> ResolveUserAsync(string? cacheKey);
    public abstract Task<List<UserOutgoingDto>> SearchUsersAsync(string query);
    public abstract Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context);
}
