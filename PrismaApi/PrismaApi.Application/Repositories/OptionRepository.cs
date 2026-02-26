using System.Linq;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class OptionRepository : BaseRepository<Option, Guid>
{
    public readonly IDiscreteTableRuleTrigger _ruleTrigger;
    public OptionRepository(AppDbContext dbContext, IDiscreteTableRuleTrigger ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public override async Task UpdateRangeAsync(IEnumerable<Option> incommingEntities)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id));
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

        await DbContext.SaveChangesAsync();
    }

    public override async Task<Option> AddAsync(Option entity)
    {
        await _ruleTrigger.ParentOptionsAddedAsync([entity.DecisionId], default);
        return await base.AddAsync(entity);
    }

    public override async Task<List<Option>> AddRangeAsync(IEnumerable<Option> entities)
    {
        await _ruleTrigger.ParentOptionsAddedAsync([.. entities.Select(e => e.DecisionId)], default);
        return await base.AddRangeAsync(entities);
    }
}
