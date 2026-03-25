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

    public async Task UpdateRangeAsync(IEnumerable<Option> incommingEntities, Expression<Func<Option, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incommingEntities.ToList();
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

            entity.DecisionId = incomingEntity.DecisionId;
            entity.Name = incomingEntity.Name;
            entity.Utility = incomingEntity.Utility;
        }

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
