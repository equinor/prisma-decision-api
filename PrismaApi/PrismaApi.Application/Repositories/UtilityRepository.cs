using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UtilityRepository : BaseRepository<Utility, Guid>
{
    public UtilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Utility> incommingEntities)
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

            entity.IssueId = incomingEntity.IssueId;
            entity.DiscreteUtilities.Update(incomingEntity.DiscreteUtilities, DbContext);
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Utility> Query()
    {
        return DbContext.Utilities
            .Include(u => u.DiscreteUtilities)
                .ThenInclude(du => du.ParentOutcomes)
            .Include(u => u.DiscreteUtilities)
                .ThenInclude(du => du.ParentOptions);
    }
}
