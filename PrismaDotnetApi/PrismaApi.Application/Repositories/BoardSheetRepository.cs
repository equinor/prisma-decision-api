using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class BoardSheetRepository : BaseRepository<BoardSheet, Guid>, IBoardSheetRepository
{
    public BoardSheetRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<BoardSheet> incomingEntities, Expression<Func<BoardSheet, bool>> filterPredicate, CancellationToken ct = default)
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
}
