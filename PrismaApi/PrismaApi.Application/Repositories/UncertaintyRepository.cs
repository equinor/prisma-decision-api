using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UncertaintyRepository : BaseRepository<Uncertainty, Guid>
{
    public UncertaintyRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Uncertainty> incommingEntities)
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
            entity.IsKey = incomingEntity.IsKey;
            entity.Outcomes = entity.Outcomes.Update(incomingEntity.Outcomes);
            entity.DiscreteProbabilities = entity.DiscreteProbabilities.Update(incomingEntity.DiscreteProbabilities);
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Uncertainty> Query()
    {
        return DbContext.Uncertainties
            .Include(u => u.Outcomes)
            .Include(u => u.DiscreteProbabilities)
                .ThenInclude(dp => dp.ParentOutcomes)
            .Include(u => u.DiscreteProbabilities)
                .ThenInclude(dp => dp.ParentOptions);
    }
}
