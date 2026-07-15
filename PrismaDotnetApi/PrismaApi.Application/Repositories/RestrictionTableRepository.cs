using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class RestrictionTableRepository : BaseRepository<RestrictionTable, Guid>, IRestrictionTableRepository
{
    public RestrictionTableRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<RestrictionTable> incomingEntities, Expression<Func<RestrictionTable, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
        if (entities.Count != incomingList.Count)
            incomingList = incomingList.Where(e => entities.Select(x => x.Id).Contains(e.Id)).ToList();

        entities.Update(incomingList, DbContext);

        await DbContext.SaveChangesAsync(ct);
    }

    protected override IQueryable<RestrictionTable> Query()
    {
        return DbContext.RestrictionTables
            .Include(e => e.RestrictionEntries);
    }
}
