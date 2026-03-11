using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface ICrudRepository<TEntity, TId>
    where TEntity : class
    where TId : struct
{
    Task<TEntity?> GetByIdAsync(TId id, bool withTracking = true, IQueryable<TEntity>? customQuery = null, Expression<Func<TEntity, bool>>? filterPredicate = null);
    Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, bool withTracking = true, IQueryable<TEntity>? customQuery = null, Expression<Func<TEntity, bool>>? filterPredicate = null);
    Task<List<TEntity>> GetAllAsync(bool withTracking = true, IQueryable<TEntity>? customQuery = null, Expression<Func<TEntity, bool>>? filterPredicate = null);
    Task<TEntity> AddAsync(TEntity entity);
    Task<List<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteByIdsAsync(IEnumerable<TId> ids, Expression<Func<TEntity, bool>>? filterPredicate = null);
}
