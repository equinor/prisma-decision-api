using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure;
using PrismaApi.Infrastructure.Extensions;
using System.Linq.Expressions;

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

    public virtual Task<TEntity?> GetByIdAsync(TId id, bool withTracking = true, IQueryable<TEntity>? customQuery = null, Expression<Func<TEntity, bool>>? filterPredicate = null)
    {
        IQueryable<TEntity> query = customQuery ?? Query();
        query = query.OptionalWhere(filterPredicate);

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    public virtual Task<List<TEntity>> GetByIdsAsync(IEnumerable<TId> ids, bool withTracking = true, IQueryable<TEntity>? customQuery = null, Expression<Func<TEntity, bool>>? filterPredicate = null)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return Task.FromResult(new List<TEntity>());
        }

        IQueryable<TEntity> query = customQuery ?? Query();
        query = query.OptionalWhere(filterPredicate);

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query
            .Where(e => idList.Contains(e.Id))
            .ToListAsync();
    }

    public virtual Task<List<TEntity>> GetAllAsync(bool withTracking = true, IQueryable<TEntity>? customQuery = null, Expression<Func<TEntity, bool>>? filterPredicate = null)
    {
        IQueryable<TEntity> query = customQuery ?? Query();
        query = query.OptionalWhere(filterPredicate);

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

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        Set.UpdateRange(entities);
        await DbContext.SaveChangesAsync();
    }

    public virtual async Task DeleteByIdsAsync(IEnumerable<TId> ids, Expression<Func<TEntity, bool>>? filterPredicate = null)
    {
        var entities = await Set
            .OptionalWhere(filterPredicate)
            .Where(e => ids.ToList().Contains(e.Id))
            .ToListAsync();

        if (entities.Count == 0)
        {
            return;
        }

        Set.RemoveRange(entities);
        await DbContext.SaveChangesAsync();
    }
}
