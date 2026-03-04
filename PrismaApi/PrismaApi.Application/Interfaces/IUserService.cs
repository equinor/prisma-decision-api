using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IUserService
{
    Task<List<UserOutgoingDto>> GetAsync(List<int> ids);
    Task<List<UserOutgoingDto>> GetAllAsync();
    Task<UserOutgoingDto?> GetByAzureIdAsync(string azureId);
    Task<UserOutgoingDto> GetOrCreateUserByAzureIdAsync(UserIncomingDto dto);
    Task<UserOutgoingDto> GetOrCreateUserFromGraphMeAsync(string? cacheKey);
}
