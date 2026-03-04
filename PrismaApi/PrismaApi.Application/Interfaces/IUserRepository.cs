using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces;

public interface IUserRepository : ICrudRepository<User, int>
{
  Task<User?> GetByAzureIdAsync(string azureId);
    Task<User> GetOrAddByAzureIdAsync(UserIncomingDto dto);
}
