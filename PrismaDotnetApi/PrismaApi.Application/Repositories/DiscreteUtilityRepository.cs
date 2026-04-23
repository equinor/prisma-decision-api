using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class DiscreteUtilityRepository : BaseRepository<DiscreteUtility, Guid>, IDiscreteUtilityRepository
{
    public DiscreteUtilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<DiscreteUtility> incomingEntities, Expression<Func<DiscreteUtility, bool>> filterPredicate, CancellationToken ct = default)
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

            entity.UtilityValue = incomingEntity.UtilityValue;
        }

        await DbContext.SaveChangesAsync(ct);
    }

    protected override IQueryable<DiscreteUtility> Query()
    {
        return DbContext.DiscreteUtilities
            .Include(du => du.ParentOutcomes)
            .Include(du => du.ParentOptions);
    }
}
