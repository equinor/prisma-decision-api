using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class StrategyRepository : BaseRepository<Strategy, Guid>, IStrategyRepository
{
    public StrategyRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<Strategy> incomingEntities, Expression<Func<Strategy, bool>> filterPredicate, CancellationToken ct = default)
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
        entities.Update(incomingList, DbContext);

        await DbContext.SaveChangesAsync(ct);
    }

    protected override IQueryable<Strategy> Query()
    {
        return DbContext.Strategies
            .Include(s => s.StrategyOptions)
                .ThenInclude(so => so.Option);
    }
}
