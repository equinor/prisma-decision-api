using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Test.Mocks;

public class TestUserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public TestUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserOutgoingDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync(withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<UserOutgoingDto> GetOrCreateUserByIdAsync(UserIncomingDto dto)
    {
        var user = await _userRepository.GetOrAddByIdAsync(dto);
        return user.ToOutgoingDto();
    }

    public async Task<UserOutgoingDto> GetOrCreateUserFromGraphMeAsync(string? cacheKey)
    {
        var id = cacheKey ?? Guid.NewGuid().ToString();
        var user = await _userRepository.GetOrAddByIdAsync(new UserIncomingDto
        {
            Id = id,
            Name = "Test User"
        });
        return user.ToOutgoingDto();
    }

    public Task<List<UserOutgoingDto>> SearchUsersFromGraphAsync(string query)
    {
        return Task.FromResult(new List<UserOutgoingDto>());
    }

    public async Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context)
    {
        var oid = context.User.Claims
            .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (string.IsNullOrEmpty(oid))
        {
            throw new InvalidOperationException("No Id found in Claims");
        }

        var name = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "Test User";
        var user = await _userRepository.GetOrAddByIdAsync(new UserIncomingDto
        {
            Id = oid,
            Name = name
        });

        return user.ToOutgoingDto();
    }

    public async Task<List<UserOutgoingDto>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var users = await _userRepository.GetByIdsAsync(ids, withTracking: false);
        return users.ToOutgoingDtos();
    }
}
