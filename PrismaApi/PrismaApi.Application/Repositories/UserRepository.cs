using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Extensions;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class UserRepository : BaseRepository<User, string>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<User> incomingEntities)
    {
        var incomingList = incomingEntities.ToList();
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

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<User> Query()
    {
        return DbContext.Users
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
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        return user;
    }
}
