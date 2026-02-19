using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class ProjectRoleRepository : BaseRepository<ProjectRole, Guid>
{
    public ProjectRoleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<ProjectRole> incommingEntities)
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

            entity.ProjectId = incomingEntity.ProjectId;
            entity.UserId = incomingEntity.UserId;
            entity.Role = incomingEntity.Role;
            entity.UpdatedById = incomingEntity.UpdatedById;
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<ProjectRole> Query()
    {
        return DbContext.ProjectRoles
            .Include(pr => pr.User)
            .Include(pr => pr.Project);
    }
}
