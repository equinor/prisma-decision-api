using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class UncertaintyRepository : BaseRepository<Uncertainty, Guid>, IUncertaintyRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public UncertaintyRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public async  Task UpdateRangeAsync(IEnumerable<Uncertainty> incomingEntities, Expression<Func<Uncertainty, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
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
            await entity.Outcomes.Update(incomingEntity.Outcomes, DbContext, ct: ct);
            entity.DiscreteProbabilities.Update(incomingEntity.DiscreteProbabilities, DbContext);
        }

        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers, ct);
        await DbContext.SaveChangesAsync(ct);
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
