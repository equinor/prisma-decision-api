using System.Linq;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class NodeStyleRepository : BaseRepository<NodeStyle, Guid>
{
    public NodeStyleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<NodeStyle> incommingEntities)
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

            entity.NodeId = incomingEntity.NodeId;
            entity.XPosition = incomingEntity.XPosition;
            entity.YPosition = incomingEntity.YPosition;
        }

        await DbContext.SaveChangesAsync();
    }
}
