using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
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

    public async Task UpdateRangeAsync(IEnumerable<Outcome> incommingEntities, Expression<Func<Outcome, bool>> filterPredicate)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate);
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

        await DbContext.SaveChangesAsync();
    }
    public override async Task<Outcome> AddAsync(Outcome entity)
    {
        await _ruleTrigger.OnUncertaintyOutcomesAddedAsync([entity.UncertaintyId], default);
        return await base.AddAsync(entity);
    }

    public override async Task<List<Outcome>> AddRangeAsync(IEnumerable<Outcome> entities)
    {
        await _ruleTrigger.OnUncertaintyOutcomesAddedAsync([.. entities.Select(e => e.UncertaintyId)], default);
        return await base.AddRangeAsync(entities);
    }
}
