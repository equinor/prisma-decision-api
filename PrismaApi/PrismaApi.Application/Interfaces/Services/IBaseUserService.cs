using Microsoft.AspNetCore.Http;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IBaseUserService
{
    Task<List<UserOutgoingDto>> GetAllAsync();
    Task<List<UserOutgoingDto>> SearchUsersAsync(string query);
    Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context);
    Task<List<UserOutgoingDto>> GetByIdsAsync(IEnumerable<string> ids);
}
