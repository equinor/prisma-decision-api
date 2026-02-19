using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class EdgeRepository : BaseRepository<Edge, Guid>
{
    public EdgeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<Edge> incommingEntities)
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

            entity.TailId = incomingEntity.TailId;
            entity.HeadId = incomingEntity.HeadId;
            entity.ProjectId = incomingEntity.ProjectId;
        }

        await DbContext.SaveChangesAsync();
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
