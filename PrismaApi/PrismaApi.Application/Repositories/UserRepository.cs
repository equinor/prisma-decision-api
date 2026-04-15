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

    public override async Task UpdateRangeAsync(IEnumerable<User> incomingEntities, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), ct: ct);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.Name = incomingEntity.Name;
        }

        await DbContext.SaveChangesAsync(ct);
    }

    protected override IQueryable<User> Query()
    {
        return DbContext.Users
            .Include(u => u.ProjectRoles);
    }

    private async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        return await Query()
            .FirstOrDefaultAsync(u => u.Name.ToLower() == userName.ToLower(), ct);
    }

    public async Task<User> GetOrAddByIdAsync(UserIncomingDto dto, CancellationToken ct = default)
    {
        var existingUser = await GetByIdAsync(dto.Id, ct: ct);
        if (existingUser != null)
        {
            return existingUser;
        }

        User user = dto.ToEntity();
        await DbContext.Users.AddAsync(user, ct);
        await DbContext.SaveChangesAsync(ct);

        return user;
    }
    public async Task<User> GetOrAddByUserNameAsync(UserIncomingDto dto, CancellationToken ct = default)
    {
        var existingUser = await GetByUserNameAsync(dto.Name, ct: ct);
        if (existingUser != null)
        {
            return existingUser;
        }

        User user = dto.ToEntity();
        await DbContext.Users.AddAsync(user, ct);
        await DbContext.SaveChangesAsync(ct);

        return user;
    }
}
