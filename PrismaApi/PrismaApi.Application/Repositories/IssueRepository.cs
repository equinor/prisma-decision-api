using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class IssueRepository : BaseRepository<Issue, Guid>, IIssueRepository
{
    public readonly IDiscreteTableRuleEventHandler _ruleTrigger;
    public readonly ITableRebuildingService _tableRebuildingService;
    public IssueRepository(AppDbContext dbContext, IDiscreteTableRuleEventHandler ruleTrigger, ITableRebuildingService tableRebuildingService) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
        _tableRebuildingService = tableRebuildingService;
    }

    public override async Task DeleteByIdsAsync(IEnumerable<Guid> ids, Expression<Func<Issue, bool>>? filterPredicate = null, CancellationToken ct = default)
    {
        var edgesToDelete = await DbContext.Edges
            .Where(x => ids.Contains(x.HeadNode!.IssueId) || ids.Contains(x.TailNode!.IssueId))
            .ToListAsync(ct);
        DbContext.Edges.RemoveRange(edgesToDelete);
        await _ruleTrigger.OnEdgesRemovedAsync(edgesToDelete.Select(x => x.Id).ToList(), ct);
        await _tableRebuildingService.RebuildTablesAsync(ct);
        await DbContext.SaveChangesAsync(ct);
        await base.DeleteByIdsAsync(ids, filterPredicate, ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<Issue> incomingEntities, Expression<Func<Issue, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
        List<Guid> issuesIdsTriggers = [];
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            if (WillIssueChangeTables(entity, incomingEntity))
                issuesIdsTriggers.Add(entity.Id);

            await entity.RemoveOutOfScopeStrategyOptions(incomingEntity, DbContext, ct);

            entity.ProjectId = incomingEntity.ProjectId;
            entity.Type = incomingEntity.Type;
            entity.Boundary = incomingEntity.Boundary;
            entity.Name = incomingEntity.Name;
            entity.Description = incomingEntity.Description;
            entity.Order = incomingEntity.Order;
            entity.UpdatedById = incomingEntity.UpdatedById;

            if (incomingEntity.Node != null && entity.Node != null)
                entity.Node = entity.Node.Update(incomingEntity.Node, ct);

            if (incomingEntity.Decision != null && entity.Decision != null)
                entity.Decision = await entity.Decision.Update(incomingEntity.Decision, DbContext, _ruleTrigger, ct);

            if (incomingEntity.Uncertainty != null && entity.Uncertainty != null)
                entity.Uncertainty = await entity.Uncertainty.Update(incomingEntity.Uncertainty, DbContext, _ruleTrigger, ct);

            if (incomingEntity.Utility != null && entity.Utility != null)
                entity.Utility = entity.Utility.Update(incomingEntity.Utility, DbContext, ct);
        }
        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers, ct);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task<ICollection<Issue>> GetIssuesInInfluenceDiagram(Guid projectId, Expression<Func<Issue, bool>>? filterPredicate, CancellationToken ct = default)
    {
        return await base.GetAllAsync(false, Query().IndluenceDiagramFilter(projectId), filterPredicate, ct);
    }

    private bool WillIssueChangeTables(Issue entity, Issue incomingEntity)
    {
        if (entity.Type != incomingEntity.Type) return true;
        if (!string.Equals(entity.Boundary, incomingEntity.Boundary, StringComparison.OrdinalIgnoreCase)
            && (incomingEntity.Boundary.Equals(Boundary.Out.ToString(), StringComparison.OrdinalIgnoreCase) || entity.Boundary.Equals(Boundary.Out.ToString(), StringComparison.OrdinalIgnoreCase))) 
            return true;
        return false;
    }

    protected override IQueryable<Issue> Query()
    {
        return DbContext.Issues
            .AsSplitQuery()
            .Include(i => i.Node!)
                .ThenInclude(n => n.NodeStyle)
            .Include(i => i.Node!)
                .ThenInclude(n => n.HeadEdges)
            .Include(i => i.Node!)
                .ThenInclude(n => n.TailEdges)
            .Include(i => i.Decision!)
                .ThenInclude(d => d.Options)
            .Include(i => i.Uncertainty!)
                .ThenInclude(u => u.Outcomes)
            .Include(i => i.Uncertainty!)
                .ThenInclude(u => u.DiscreteProbabilities)
                    .ThenInclude(d => d.ParentOptions)
            .Include(i => i.Uncertainty!)
                .ThenInclude(u => u.DiscreteProbabilities)
                    .ThenInclude(d => d.ParentOutcomes)
            .Include(i => i.Utility)
                .ThenInclude(u => u!.DiscreteUtilities)
                    .ThenInclude(d => d.ParentOptions)
            .Include(i => i.Utility)
                .ThenInclude(u => u!.DiscreteUtilities)
                    .ThenInclude(d => d.ParentOutcomes);

    }
}

public static class IssueQueryableExtensions
{
    public static IQueryable<Issue> IndluenceDiagramFilter(this IQueryable<Issue> query, Guid projectId)
    {
        return query
            .Where(e =>
                e.ProjectId == projectId &&
                (e.Boundary.ToUpper() == Boundary.In.ToString().ToUpper() || e.Boundary.ToUpper() == Boundary.On.ToString().ToUpper()) &&
                (e.Type == IssueType.Uncertainty.ToString() || e.Type == IssueType.Decision.ToString() || e.Type == IssueType.Utility.ToString()) &&
                (
                    (e.Type == IssueType.Uncertainty.ToString() && e.Uncertainty!.IsKey == true) ||
                    (e.Type == IssueType.Decision.ToString() && e.Decision!.Type == DecisionHierarchy.Focus.ToString()) ||
                    (e.Type == IssueType.Utility.ToString())
                )
            );
    }
}
