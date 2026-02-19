using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class NodeRepository : BaseRepository<Node, Guid>
{
    public NodeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Node> incommingEntities)
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

            entity.ProjectId = incomingEntity.ProjectId;
            entity.IssueId = incomingEntity.IssueId;
            entity.Name = incomingEntity.Name;
            entity.HeadEdges = entity.HeadEdges.Update(incomingEntity.HeadEdges);
            entity.TailEdges = entity.TailEdges.Update(incomingEntity.TailEdges);
            if (entity.NodeStyle != null && incomingEntity.NodeStyle != null)
                entity.NodeStyle = entity.NodeStyle.Update(incomingEntity.NodeStyle);
        }

        await DbContext.SaveChangesAsync();
    }

    protected override IQueryable<Node> Query()
    {
        return DbContext.Nodes
            .Include(n => n.NodeStyle)
            .Include(n => n.Issue!)
                .ThenInclude(i => i.Decision!)
                .ThenInclude(d => d.Options)
            .Include(n => n.Issue!)
                .ThenInclude(i => i.Uncertainty!)
                .ThenInclude(u => u.Outcomes)
            .Include(n => n.Issue!)
                .ThenInclude(i => i.Utility);
    }
}
