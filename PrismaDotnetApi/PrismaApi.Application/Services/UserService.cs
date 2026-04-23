using Microsoft.AspNetCore.Http;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProvider _userProvider;

    public UserService(IUserRepository userRepository, IUserProvider userProvider)
    {
        _userRepository = userRepository;
        _userProvider = userProvider;
    }

    public async Task<List<UserOutgoingDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync(withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<List<UserOutgoingDto>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var users = await _userRepository.GetByIdsAsync(ids, withTracking: false);
        return users.ToOutgoingDtos();
    }

    public Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context)
        => _userProvider.ResolveUserFromContextAsync(context);

    public Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
        => _userProvider.SearchUsersAsync(query);
}
