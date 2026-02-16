using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class EdgeRepository : BaseRepository<Edge, System.Guid>
{
    public EdgeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Edge> Query()
    {
        return DbContext.Edges
            .Include(e => e.HeadNode)
                .ThenInclude(n => n!.NodeStyle)
            .Include(e => e.HeadNode)
                .ThenInclude(n => n!.Issue)
                .ThenInclude(i => i!.Decision)
                .ThenInclude(d => d!.Options)
            .Include(e => e.HeadNode)
                .ThenInclude(n => n!.Issue)
                .ThenInclude(i => i!.Uncertainty)
                .ThenInclude(u => u!.Outcomes)
            .Include(e => e.HeadNode)
                .ThenInclude(n => n!.Issue)
                .ThenInclude(i => i!.Utility)
            .Include(e => e.TailNode)
                .ThenInclude(n => n!.NodeStyle)
            .Include(e => e.TailNode)
                .ThenInclude(n => n!.Issue)
                .ThenInclude(i => i!.Decision)
                .ThenInclude(d => d!.Options)
            .Include(e => e.TailNode)
                .ThenInclude(n => n!.Issue)
                .ThenInclude(i => i!.Uncertainty)
                .ThenInclude(u => u!.Outcomes)
            .Include(e => e.TailNode)
                .ThenInclude(n => n!.Issue)
                .ThenInclude(i => i!.Utility);
    }
}
