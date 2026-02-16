using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class BaseRepository<TEntity, TId> : ICrudRepository<TEntity, TId>
    where TEntity : class
{
    protected readonly AppDbContext DbContext;
    protected readonly DbSet<TEntity> Set;

    public BaseRepository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        Set = dbContext.Set<TEntity>();
    }

    protected virtual IQueryable<TEntity> Query()
    {
        return Set.AsQueryable();
    }

    public virtual Task<TEntity?> GetByIdAsync(TId id)
    {
        return Query()
            .Where(e => EF.Property<TId>(e, "Id") == id)
            .ToListAsync();
    }

    public virtual Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return Task.FromResult(new List<TEntity>());
        }

        return Query()
            .Where(e => idList.Contains(EF.Property<TId>(e, "Id")))
            .ToListAsync();
    }

    public virtual Task<List<TEntity>> GetAllAsync()
    {
        return Query().ToListAsync();
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        Set.Add(entity);
        await DbContext.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<List<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        var list = entities.ToList();
        Set.AddRange(list);
        await DbContext.SaveChangesAsync();
        return list;
    }

    public virtual async Task<TEntity> GetOrAddAsync(TEntity entity)
    {
        existingEntity = GetByIdsAsync
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        Set.UpdateRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public virtual async Task DeleteByIdsAsync(IEnumerable<TId> ids)
    {
        var entities = await GetByIdsAsync(ids);
        if (entities.Count == 0)
        {
            return;
        }

        Set.RemoveRange(entities);
        await DbContext.SaveChangesAsync();
    }
}
