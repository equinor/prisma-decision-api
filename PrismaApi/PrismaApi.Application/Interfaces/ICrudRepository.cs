using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrismaApi.Application.Interfaces;

public interface ICrudRepository<TEntity, TId>
    where TEntity : class
    where TId : struct
{
    Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task<List<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteByIdsAsync(IEnumerable<TId> ids);
}
