using System.Linq;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class OutcomeRepository : BaseRepository<Outcome, Guid>
{
    public OutcomeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Outcome> incommingEntities)
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

            entity.UncertaintyId = incomingEntity.UncertaintyId;
            entity.Name = incomingEntity.Name;
            entity.Utility = incomingEntity.Utility;
        }

        await DbContext.SaveChangesAsync();
    }
}
