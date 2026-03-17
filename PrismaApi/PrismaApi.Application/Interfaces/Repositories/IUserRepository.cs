using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, bool withTracking = true, IQueryable<User>? customQuery = null, Expression<Func<User, bool>>? filterPredicate = null);
    Task<List<User>> GetByIdsAsync(IEnumerable<string> ids, bool withTracking = true, IQueryable<User>? customQuery = null, Expression<Func<User, bool>>? filterPredicate = null);
    Task<List<User>> GetAllAsync(bool withTracking = true, IQueryable<User>? customQuery = null, Expression<Func<User, bool>>? filterPredicate = null);
    Task<User> AddAsync(User entity);
    Task<List<User>> AddRangeAsync(IEnumerable<User> entities);
    Task UpdateRangeAsync(IEnumerable<User> entities);
    Task DeleteByIdsAsync(IEnumerable<string> ids, Expression<Func<User, bool>>? filterPredicate = null);
    Task<User> GetOrAddByIdAsync(UserIncomingDto dto);
}
