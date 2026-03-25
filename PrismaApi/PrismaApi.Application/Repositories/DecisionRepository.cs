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

    public async Task UpdateRangeAsync(IEnumerable<Decision> incommingEntities, Expression<Func<Decision, bool>> filterPredicate, CancellationToken ct = default)
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
            await entity.Update(incomingEntity, DbContext);
        }
        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Decision> Query()
    {
        return DbContext.Decisions
            .Include(d => d.Options);
    }
}
