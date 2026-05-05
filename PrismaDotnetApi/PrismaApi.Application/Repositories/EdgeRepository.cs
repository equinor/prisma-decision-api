using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
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

    public async Task UpdateRangeAsync(IEnumerable<Edge> incomingEntities, Expression<Func<Edge, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
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

        await _ruleTrigger.OnNodeConnectionsChangedAsync(nodeIdsToLookup, ct);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task<ICollection<Edge>> GetEdgesInInfluenceDiagram(Guid projectId, Expression<Func<Edge, bool>>? filterPredicate, CancellationToken ct = default)
    {
        return await base.GetAllAsync(false, Query().IndluenceDiagramFilter(projectId), filterPredicate, ct);
    }

    public override async Task<Edge> AddAsync(Edge entity, CancellationToken ct = default)
    {
        var res = await base.AddAsync(entity, ct);
        await _ruleTrigger.OnEdgesCreatedAsync([entity.Id], ct);
        return res;
    }

    public override async Task<List<Edge>> AddRangeAsync(IEnumerable<Edge> entities, CancellationToken ct = default)
    {
        var res = await base.AddRangeAsync(entities, ct);
        await _ruleTrigger.OnEdgesCreatedAsync(entities.Select(x => x.Id).ToList(), ct);
        return res;
    }

    public override async Task DeleteByIdsAsync(IEnumerable<Guid> ids, Expression<Func<Edge, bool>>? filterPredicate = null, CancellationToken ct = default)
    {
        await _ruleTrigger.OnEdgesRemovedAsync(ids.ToList(), ct);
        await base.DeleteByIdsAsync(ids, filterPredicate, ct);
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
                (e.TailNode!.Issue!.Boundary.ToUpper() == Boundary.In.ToString().ToUpper() || e.TailNode.Issue.Boundary.ToUpper() == Boundary.On.ToString().ToUpper()) &&
                (e.TailNode.Issue.Type == IssueType.Uncertainty.ToString() || e.TailNode.Issue.Type == IssueType.Decision.ToString() || e.TailNode.Issue.Type == IssueType.Utility.ToString()) &&
                (
                    (e.TailNode.Issue.Type == IssueType.Uncertainty.ToString() && e.TailNode.Issue.Uncertainty!.IsKey == true) ||
                    (e.TailNode.Issue.Type == IssueType.Decision.ToString() && e.TailNode.Issue.Decision!.Type == DecisionHierarchy.Focus.ToString()) ||
                    (e.TailNode.Issue.Type == IssueType.Utility.ToString())
                ) &&
                (e.HeadNode!.Issue!.Boundary.ToUpper() == Boundary.In.ToString().ToUpper() || e.HeadNode.Issue.Boundary.ToUpper() == Boundary.On.ToString().ToUpper()) &&
                (e.HeadNode.Issue.Type == IssueType.Uncertainty.ToString() || e.HeadNode.Issue.Type == IssueType.Decision.ToString() || e.HeadNode.Issue.Type == IssueType.Utility.ToString()) &&
                (
                    (e.HeadNode.Issue.Type == IssueType.Uncertainty.ToString() && e.HeadNode.Issue.Uncertainty!.IsKey == true) ||
                    (e.HeadNode.Issue.Type == IssueType.Decision.ToString() && e.HeadNode.Issue.Decision!.Type == DecisionHierarchy.Focus.ToString()) ||
                    (e.HeadNode.Issue.Type == IssueType.Utility.ToString())
                )
            );
    }
}
