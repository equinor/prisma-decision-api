using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class DiscreteProbabilityRepository : BaseRepository<DiscreteProbability, Guid>, IDiscreteProbabilityRepository
{
    public DiscreteProbabilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<DiscreteProbability> incomingEntities, Expression<Func<DiscreteProbability, bool>> filterPredicate)
    {
        var incomingList = incomingEntities.ToList();
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

            entity.Probability = incomingEntity.Probability;
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<DiscreteProbability> Query()
    {
        return DbContext.DiscreteProbabilities
            .Include(dp => dp.ParentOutcomes)
            .Include(dp => dp.ParentOptions);
    }
}
