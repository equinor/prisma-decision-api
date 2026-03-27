using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class ObjectiveRepository : BaseRepository<Objective, Guid>, IObjectiveRepository
{
    public ObjectiveRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<Objective> incomingEntities, Expression<Func<Objective, bool>> filterPredicate, CancellationToken ct = default)
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
}
