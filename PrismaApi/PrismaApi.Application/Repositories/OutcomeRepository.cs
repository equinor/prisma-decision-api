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

    public async Task UpdateRangeAsync(IEnumerable<Outcome> incomingEntities, Expression<Func<Outcome, bool>> filterPredicate)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate);
        // filter out entities not found
        if (entities.Count != incomingList.Count)
            incomingList = incomingList.Where(e => entities.Select(x => x.Id).Contains(e.Id)).ToList();
        await entities.Update(incomingList, DbContext);

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
