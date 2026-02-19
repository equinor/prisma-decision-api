using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using PrismaApi.Application.Interfaces;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class BaseRepository<TEntity, TId> : ICrudRepository<TEntity, TId>
    where TEntity : class, IBaseEntity<TId>
    where TId : struct
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

    public virtual Task<TEntity?> GetByIdAsync(TId id, bool withTracking = true)
    {
        var query = Query();

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    public virtual Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, bool withTracking = true)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return Task.FromResult(new List<TEntity>());
        }

        var query = Query();

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query
            .Where(e => idList.Contains(e.Id))
            .ToListAsync();
    }

    public virtual Task<List<TEntity>> GetAllAsync(bool withTracking = true)
    {
        var query = Query();

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }
        return query.ToListAsync();
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
        var existingEntity = await GetByIdAsync(entity.Id);
        if (existingEntity != null)
        {
            return existingEntity;
        }

        return await AddAsync(entity);
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
