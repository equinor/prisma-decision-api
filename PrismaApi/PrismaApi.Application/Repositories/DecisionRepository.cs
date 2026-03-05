using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class DecisionRepository : BaseRepository<Decision, Guid>, IDecisionRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public DecisionRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger) : base(dbContext)
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

            await RemoveOutOfScopeStrategyOptions(entity, incomingEntity);

            if (entity.Type != incomingEntity.Type && incomingEntity.Type != DecisionHierarchy.Focus.ToString())
                issuesIdsTriggers.Add(entity.IssueId);

            entity.IssueId = incomingEntity.IssueId;
            entity.Type = incomingEntity.Type;
            await entity.Options.Update(incomingEntity.Options, DbContext);
        }
        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers);
        await DbContext.SaveChangesAsync();
    }

    private async Task RemoveOutOfScopeStrategyOptions(Decision entity, Decision incommingEntity)
    {
        if (!IsDecisionMovedOutOfStrategyTable(entity, incommingEntity)) return;
        var strategyOptionsToBeRemoved = await DbContext.StrategyOptions
            .Where(e => e.Option!.DecisionId == entity.Id)
            .ToListAsync();
        if (strategyOptionsToBeRemoved.Any())
        {
            DbContext.StrategyOptions.RemoveRange(strategyOptionsToBeRemoved);
            await DbContext.SaveChangesAsync();
        }
    }

    private static bool IsDecisionMovedOutOfStrategyTable(Decision entity, Decision incommingEntity)
    {
        if (entity.Type != incommingEntity.Type && entity.Type == DecisionHierarchy.Focus.ToString()) return true;
        return false;
    }

    protected override IQueryable<Decision> Query()
    {
        return DbContext.Decisions
            .Include(d => d.Options);
    }
}
