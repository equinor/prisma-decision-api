using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class DecisionRepository : BaseRepository<Decision, Guid>
{
    public readonly IDiscreteTableRuleTrigger _ruleTrigger;
    public DecisionRepository(AppDbContext dbContext, IDiscreteTableRuleTrigger ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public override async Task UpdateRangeAsync(IEnumerable<Decision> incommingEntities)
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

            if (entity.Type != incomingEntity.Type && incomingEntity.Type != "Foucus")
                issuesIdsTriggers.Add(entity.IssueId);

            entity.IssueId = incomingEntity.IssueId;
            entity.Type = incomingEntity.Type;
            await entity.Options.Update(incomingEntity.Options, DbContext);
        }
        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers);
        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Decision> Query()
    {
        return DbContext.Decisions
            .Include(d => d.Options);
    }
}
