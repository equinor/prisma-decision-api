using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrismaApi.Application.Interfaces;

public interface ICrudRepository<TEntity, TId>
    where TEntity : class
    where TId : struct
{
    Task<TEntity?> GetByIdAsync(TId id, bool withTracking = true, IQueryable<TEntity>? customQuery = null);
    Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, bool withTracking = true, IQueryable<TEntity>? customQuery = null);
    Task<List<TEntity>> GetAllAsync(bool withTracking = true, IQueryable<TEntity>? customQuery = null);
    Task<TEntity> AddAsync(TEntity entity);
    Task<List<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteByIdsAsync(IEnumerable<TId> ids);
    Task<TEntity> GetOrAddAsync(TEntity entity);
}
