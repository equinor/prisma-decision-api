using Microsoft.AspNetCore.Http;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IUserService
{
    Task<List<UserOutgoingDto>> GetAsync(List<string> ids);
    Task<List<UserOutgoingDto>> GetAllAsync();
    Task<UserOutgoingDto> GetOrCreateUserByIdAsync(UserIncomingDto dto);
    Task<UserOutgoingDto> GetOrCreateUserFromGraphMeAsync(string? cacheKey);
    Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context);
}
