using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class OutcomeRepository : BaseRepository<Outcome, Guid>, IOutcomeRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public OutcomeRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public async Task UpdateRangeAsync(IEnumerable<Outcome> incomingEntities, Expression<Func<Outcome, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.UncertaintyId = incomingEntity.UncertaintyId;
            entity.Name = incomingEntity.Name;
            entity.Utility = incomingEntity.Utility;
        }

        await DbContext.SaveChangesAsync(ct);
    }
    public override async Task<Outcome> AddAsync(Outcome entity, CancellationToken ct = default)
    {
        await _ruleTrigger.OnUncertaintyOutcomesAddedAsync([entity.UncertaintyId], ct);
        return await base.AddAsync(entity, ct);
    }

    public override async Task<List<Outcome>> AddRangeAsync(IEnumerable<Outcome> entities, CancellationToken ct = default)
    {
        await _ruleTrigger.OnUncertaintyOutcomesAddedAsync([.. entities.Select(e => e.UncertaintyId)], ct);
        return await base.AddRangeAsync(entities, ct);
    }
}
