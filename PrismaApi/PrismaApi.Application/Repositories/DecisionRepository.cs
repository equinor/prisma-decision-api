using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class DecisionRepository : BaseRepository<Decision, Guid>, IDecisionRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public DecisionRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public async Task UpdateRangeAsync(IEnumerable<Decision> incomingEntities, Expression<Func<Decision, bool>> filterPredicate, CancellationToken ct = default)
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

            await RemoveOutOfScopeStrategyOptions(entity, incomingEntity, ct);

            if (entity.Type != incomingEntity.Type && incomingEntity.Type != DecisionHierarchy.Focus.ToString())
                issuesIdsTriggers.Add(entity.IssueId);

            entity.IssueId = incomingEntity.IssueId;
            entity.Type = incomingEntity.Type;
            await entity.Options.Update(incomingEntity.Options, DbContext, ct: ct);
        }
        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers, ct);
        await DbContext.SaveChangesAsync(ct);
    }

    private async Task RemoveOutOfScopeStrategyOptions(Decision entity, Decision incomingEntity, CancellationToken ct = default)
    {
        if (!IsDecisionMovedOutOfStrategyTable(entity, incomingEntity)) return;
        var strategyOptionsToBeRemoved = await DbContext.StrategyOptions
            .Where(e => e.Option!.DecisionId == entity.Id)
            .ToListAsync(ct);
        if (strategyOptionsToBeRemoved.Any())
        {
            DbContext.StrategyOptions.RemoveRange(strategyOptionsToBeRemoved);
            await DbContext.SaveChangesAsync(ct);
        }
    }

    private static bool IsDecisionMovedOutOfStrategyTable(Decision entity, Decision incomingEntity)
    {
        if (entity.Type != incomingEntity.Type && entity.Type == DecisionHierarchy.Focus.ToString()) return true;
        return false;
    }

    protected override IQueryable<Decision> Query()
    {
        return DbContext.Decisions
            .Include(d => d.Options);
    }
}
