using System.Linq;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class OptionRepository : BaseRepository<Option, Guid>
{
    public OptionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Option> incommingEntities)
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

            entity.DecisionId = incomingEntity.DecisionId;
            entity.Name = incomingEntity.Name;
            entity.Utility = incomingEntity.Utility;
        }

        await DbContext.SaveChangesAsync();
    }
}
