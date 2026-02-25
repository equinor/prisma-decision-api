using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class EdgeRepository : BaseRepository<Edge, Guid>
{
    public readonly IDiscreteTableRuleTrigger _ruleTrigger;
    public EdgeRepository(AppDbContext dbContext, IDiscreteTableRuleTrigger ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public override async Task UpdateRangeAsync(IEnumerable<Edge> incommingEntities)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id));
        HashSet<Guid> nodeIdsToLookup = [];
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            if (entity.HeadId != incomingEntity.HeadId || entity.TailId != incomingEntity.TailId)
                nodeIdsToLookup.Add(entity.HeadId);
                nodeIdsToLookup.Add(incomingEntity.HeadId);
            
            entity.TailId = incomingEntity.TailId;
            entity.HeadId = incomingEntity.HeadId;
            entity.ProjectId = incomingEntity.ProjectId;
        }

        await _ruleTrigger.NodesConnectionsChangeAsync(nodeIdsToLookup);
        await DbContext.SaveChangesAsync();
    }

    public override async Task<Edge> AddAsync(Edge entity)
    {
        var res = await base.AddAsync(entity);
        await _ruleTrigger.EdgesAddedAsync([entity.Id]);
        return res;
    }

    public override async Task<List<Edge>> AddRangeAsync(IEnumerable<Edge> entities)
    {
        var res = await base.AddRangeAsync(entities);
        await _ruleTrigger.EdgesAddedAsync(entities.Select(x => x.Id).ToList());
        return res;
    }

    public override async Task DeleteByIdsAsync(IEnumerable<Guid> ids)
    {
        await _ruleTrigger.EdgesDeletedAsync(ids.ToList());
        await base.DeleteByIdsAsync(ids);
    }

    protected override IQueryable<Edge> Query()
    {
        return DbContext.Edges
            .Include(e => e.HeadNode)
            .Include(e => e.TailNode);
            //.Include(e => e.HeadNode)
            //    .ThenInclude(n => n!.NodeStyle)
            //.Include(e => e.HeadNode)
            //    .ThenInclude(n => n!.Issue)
            //    .ThenInclude(i => i!.Decision)
            //    .ThenInclude(d => d!.Options)
            //.Include(e => e.HeadNode)
            //    .ThenInclude(n => n!.Issue)
            //    .ThenInclude(i => i!.Uncertainty)
            //    .ThenInclude(u => u!.Outcomes)
            //.Include(e => e.HeadNode)
            //    .ThenInclude(n => n!.Issue)
            //    .ThenInclude(i => i!.Utility)
            //.Include(e => e.TailNode)
            //    .ThenInclude(n => n!.NodeStyle)
            //.Include(e => e.TailNode)
            //    .ThenInclude(n => n!.Issue)
            //    .ThenInclude(i => i!.Decision)
            //    .ThenInclude(d => d!.Options)
            //.Include(e => e.TailNode)
            //    .ThenInclude(n => n!.Issue)
            //    .ThenInclude(i => i!.Uncertainty)
            //    .ThenInclude(u => u!.Outcomes)
            //.Include(e => e.TailNode)
            //    .ThenInclude(n => n!.Issue)
            //    .ThenInclude(i => i!.Utility);
    }
}
