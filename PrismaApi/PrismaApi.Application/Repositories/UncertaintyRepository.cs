using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UncertaintyRepository : BaseRepository<Uncertainty, Guid>
{
    public readonly IDiscreteTableRuleTrigger _ruleTrigger;
    public UncertaintyRepository(AppDbContext dbContext, IDiscreteTableRuleTrigger ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public override async Task UpdateRangeAsync(IEnumerable<Uncertainty> incommingEntities)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id));
        List<Guid> issuesIdsTriggers = [];
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }
            if (entity.IsKey != incomingEntity.IsKey)
                issuesIdsTriggers.Add(entity.IssueId);
            entity.IssueId = incomingEntity.IssueId;
            entity.IsKey = incomingEntity.IsKey;
            await entity.Outcomes.Update(incomingEntity.Outcomes, DbContext);
            entity.DiscreteProbabilities.Update(incomingEntity.DiscreteProbabilities, DbContext);
        }

        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers);
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
