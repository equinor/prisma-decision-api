using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UserRepository : BaseRepository<User, int>
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<User> incommingEntities)
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
            entity.AzureId = incomingEntity.AzureId;
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<User> Query()
    {
        return DbContext.Users
            .Include(u => u.ProjectRoles);
    }

    public Task<User?> GetByAzureIdAsync(string azureId)
    {
        return DbContext.Users
            .AsNoTracking()
            .Include(u => u.ProjectRoles)
            .FirstOrDefaultAsync(u => u.AzureId == azureId);
    }

    public async Task<User> GetOrAddByAzureIdAsync(UserIncomingDto dto)
    {
        var existingUser = await DbContext.Users
            .AsNoTracking()
            .Include(u => u.ProjectRoles)
            .FirstOrDefaultAsync(u => u.AzureId == dto.AzureId);

        if (existingUser != null)
        {
            return existingUser;
        }

        User user = dto.ToEntity();
        await DbContext.Users.AddAsync(user);

        return user;
    }
}
