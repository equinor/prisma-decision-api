using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class EdgeRepository : BaseRepository<Edge, Guid>, IEdgeRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public EdgeRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public async Task UpdateRangeAsync(IEnumerable<Edge> incommingEntities, Expression<Func<Edge, bool>> filterPredicate)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate);
        HashSet<Guid> nodeIdsToLookup = [];
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            if (entity.HeadId != incomingEntity.HeadId || entity.TailId != incomingEntity.TailId)
            {
                nodeIdsToLookup.Add(entity.TailId);
                nodeIdsToLookup.Add(incomingEntity.TailId);
            }

            entity.TailId = incomingEntity.TailId;
            entity.HeadId = incomingEntity.HeadId;
            entity.ProjectId = incomingEntity.ProjectId;
        }

        await _ruleTrigger.OnNodeConnectionsChangedAsync(nodeIdsToLookup);
        await DbContext.SaveChangesAsync();
    }

    public async Task<ICollection<Edge>> GetEdgesInInfluenceDiagram(Guid projectId, Expression<Func<Edge, bool>>? filterPredicate)
    {
        return await base.GetAllAsync(false, Query().IndluenceDiagramFilter(projectId), filterPredicate);
    }

    public override async Task<Edge> AddAsync(Edge entity)
    {
        var res = await base.AddAsync(entity);
        await _ruleTrigger.OnEdgesCreatedAsync([entity.Id]);
        return res;
    }

    public override async Task<List<Edge>> AddRangeAsync(IEnumerable<Edge> entities)
    {
        var res = await base.AddRangeAsync(entities);
        await _ruleTrigger.OnEdgesCreatedAsync(entities.Select(x => x.Id).ToList());
        return res;
    }

    public override async Task DeleteByIdsAsync(IEnumerable<Guid> ids, Expression<Func<Edge, bool>>? filterPredicate = null)
    {
        await _ruleTrigger.OnEdgesRemovedAsync(ids.ToList());
        await base.DeleteByIdsAsync(ids, filterPredicate);
    }

    protected override IQueryable<Edge> Query()
    {
        return DbContext.Edges
            .Include(e => e.HeadNode)
            .Include(e => e.TailNode);
    }
}

public static class EdgeQueryableExtensions
{
    public static IQueryable<Edge> IndluenceDiagramFilter(this IQueryable<Edge> query, Guid projectId)
    {
        return query
            .Where(e =>
                e.ProjectId == projectId &&
                (e.TailNode!.Issue!.Boundary == Boundary.In.ToString() || e.TailNode.Issue.Boundary == Boundary.On.ToString()) &&
                (e.TailNode.Issue.Type == IssueType.Uncertainty.ToString() || e.TailNode.Issue.Type == IssueType.Decision.ToString() || e.TailNode.Issue.Type == IssueType.Utility.ToString()) &&
                (
                    (e.TailNode.Issue.Type == IssueType.Uncertainty.ToString() && e.TailNode.Issue.Uncertainty!.IsKey == true) ||
                    (e.TailNode.Issue.Type == IssueType.Decision.ToString() && e.TailNode.Issue.Decision!.Type == DecisionHierarchy.Focus.ToString()) ||
                    (e.TailNode.Issue.Type == IssueType.Utility.ToString())
                ) &&
                (e.HeadNode!.Issue!.Boundary == Boundary.In.ToString() || e.HeadNode.Issue.Boundary == Boundary.On.ToString()) &&
                (e.HeadNode.Issue.Type == IssueType.Uncertainty.ToString() || e.HeadNode.Issue.Type == IssueType.Decision.ToString() || e.HeadNode.Issue.Type == IssueType.Utility.ToString()) &&
                (
                    (e.HeadNode.Issue.Type == IssueType.Uncertainty.ToString() && e.HeadNode.Issue.Uncertainty!.IsKey == true) ||
                    (e.HeadNode.Issue.Type == IssueType.Decision.ToString() && e.HeadNode.Issue.Decision!.Type == DecisionHierarchy.Focus.ToString()) ||
                    (e.HeadNode.Issue.Type == IssueType.Utility.ToString())
                )
            );
    }
}
