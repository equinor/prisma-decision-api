using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class NodeStyleRepository : BaseRepository<NodeStyle, Guid>, INodeStyleRepository
{
    public NodeStyleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<NodeStyle> incomingEntities, Expression<Func<NodeStyle, bool>> filterPredicate, CancellationToken ct = default)
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

            entity.NodeId = incomingEntity.NodeId;
            entity.XPosition = incomingEntity.XPosition;
            entity.YPosition = incomingEntity.YPosition;
        }

        await DbContext.SaveChangesAsync(ct);
    }
}
