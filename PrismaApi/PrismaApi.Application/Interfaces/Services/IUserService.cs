using Microsoft.AspNetCore.Http;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IUserService
{
    Task<List<UserOutgoingDto>> GetAllAsync();
    Task<UserOutgoingDto> GetOrCreateUserByIdAsync(UserIncomingDto dto);
    Task<UserOutgoingDto> GetOrCreateUserFromGraphMeAsync(string? cacheKey);
    Task<List<UserOutgoingDto>> SearchUsersFromGraphAsync(string query);

    Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context);
    Task<List<UserOutgoingDto>> GetByIdsAsync(IEnumerable<string> ids);
}
