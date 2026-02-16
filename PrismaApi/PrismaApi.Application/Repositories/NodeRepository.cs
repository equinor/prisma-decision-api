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
