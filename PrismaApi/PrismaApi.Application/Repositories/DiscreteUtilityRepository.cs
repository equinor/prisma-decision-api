using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class DiscreteUtilityRepository : BaseRepository<DiscreteUtility, Guid>
{
    public DiscreteUtilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<DiscreteUtility> incomingEntities)
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

            entity.UtilityValue = incomingEntity.UtilityValue;
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<DiscreteUtility> Query()
    {
        return DbContext.DiscreteUtilities
            .Include(du => du.ParentOutcomes)
            .Include(du => du.ParentOptions);
    }
}
