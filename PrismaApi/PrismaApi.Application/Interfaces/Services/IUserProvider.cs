using Microsoft.AspNetCore.Http;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IUserProvider
{
    Task<UserOutgoingDto> ResolveUserFromContextAsync(HttpContext context);
    Task<List<UserOutgoingDto>> SearchUsersAsync(string query);
}
