using System.Linq;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class ObjectiveRepository : BaseRepository<Objective, Guid>
{
    public ObjectiveRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Objective> incommingEntities)
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
            entity.Name = incomingEntity.Name;
            entity.Type = incomingEntity.Type;
            entity.Description = incomingEntity.Description;
            entity.UpdatedById = incomingEntity.UpdatedById;
        }

        await DbContext.SaveChangesAsync();
    }
}
