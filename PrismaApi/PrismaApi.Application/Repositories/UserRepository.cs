using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Extensions;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual Task<User?> GetByIdAsync(string id, bool withTracking = true, IQueryable<User>? customQuery = null, Expression<Func<User, bool>>? filterPredicate = null)
    {
        IQueryable<User> query = customQuery ?? Query();
        query = query.OptionalWhere(filterPredicate);

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    public virtual Task<List<User>> GetByIdsAsync(IEnumerable<string> ids, bool withTracking = true, IQueryable<User>? customQuery = null, Expression<Func<User, bool>>? filterPredicate = null)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return Task.FromResult(new List<User>());
        }

        IQueryable<User> query = customQuery ?? Query();
        query = query.OptionalWhere(filterPredicate);

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }

        return query
            .Where(e => idList.Contains(e.Id))
            .ToListAsync();
    }

    public virtual Task<List<User>> GetAllAsync(bool withTracking = true, IQueryable<User>? customQuery = null, Expression<Func<User, bool>>? filterPredicate = null)
    {
        IQueryable<User> query = customQuery ?? Query();
        query = query.OptionalWhere(filterPredicate);

        if (!withTracking)
        {
            query = query.AsNoTracking();
        }
        return query.ToListAsync();
    }

    public virtual async Task<User> AddAsync(User entity)
    {
        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<List<User>> AddRangeAsync(IEnumerable<User> entities)
    {
        var list = entities.ToList();
        _dbContext.Users.AddRange(list);
        await _dbContext.SaveChangesAsync();
        return list;
    }

    public virtual async Task DeleteByIdsAsync(IEnumerable<string> ids, Expression<Func<User, bool>>? filterPredicate = null)
    {
        var entities = await _dbContext.Users
            .OptionalWhere(filterPredicate)
            .Where(e => ids.ToList().Contains(e.Id))
            .ToListAsync();

        if (entities.Count == 0)
        {
            return;
        }

        _dbContext.Users.RemoveRange(entities);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<User> incommingEntities)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id));
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.Name = incomingEntity.Name;
        }

        await _dbContext.SaveChangesAsync();
    }

    protected IQueryable<User> Query()
    {
        return _dbContext.Users
            .Include(u => u.ProjectRoles);
    }

    public async Task<User> GetOrAddByIdAsync(UserIncomingDto dto)
    {
        var existingUser = await GetByIdAsync(dto.Id);
        if (existingUser != null)
        {
            return existingUser;
        }

        User user = dto.ToEntity();
        await _dbContext.Users.AddAsync(user);

        return user;
    }
}
