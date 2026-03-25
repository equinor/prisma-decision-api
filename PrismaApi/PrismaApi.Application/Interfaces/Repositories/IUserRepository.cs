using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUserRepository : ICrudRepository<User, string>
{
    Task<User> GetOrAddByIdAsync(UserIncomingDto dto, CancellationToken ct = default);
}
