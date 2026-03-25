using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class UtilityRepository : BaseRepository<Utility, Guid>, IUtilityRepository
{
    public UtilityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<Utility> incommingEntities, Expression<Func<Utility, bool>> filterPredicate, CancellationToken ct = default)
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
            entity.Update(incomingEntity, DbContext);
        }

        await DbContext.SaveChangesAsync(ct);
    }

    protected override IQueryable<Utility> Query()
    {
        return DbContext.Utilities
            .Include(u => u.DiscreteUtilities)
                .ThenInclude(du => du.ParentOutcomes)
            .Include(u => u.DiscreteUtilities)
                .ThenInclude(du => du.ParentOptions);
    }
}
