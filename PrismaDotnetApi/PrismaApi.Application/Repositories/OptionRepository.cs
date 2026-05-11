using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class OptionRepository : BaseRepository<Option, Guid>, IOptionRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public OptionRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public async Task UpdateRangeAsync(IEnumerable<Option> incomingEntities, Expression<Func<Option, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
        // filter out entities not found
        if (entities.Count != incomingList.Count)
            incomingList = incomingList.Where(e => entities.Select(x => x.Id).Contains(e.Id)).ToList();
        await entities.Update(incomingList, DbContext, ct: ct);

        await DbContext.SaveChangesAsync(ct);
    }

    public override async Task<Option> AddAsync(Option entity, CancellationToken ct = default)
    {
        await _ruleTrigger.OnDecisionOptionsAddedAsync([entity.DecisionId], ct);
        return await base.AddAsync(entity, ct);
    }

    public override async Task<List<Option>> AddRangeAsync(IEnumerable<Option> entities, CancellationToken ct = default)
    {
        await _ruleTrigger.OnDecisionOptionsAddedAsync([.. entities.Select(e => e.DecisionId)], ct);
        return await base.AddRangeAsync(entities, ct);
    }
}
