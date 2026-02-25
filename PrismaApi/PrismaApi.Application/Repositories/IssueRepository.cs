using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using PrismaApi.Application.Repositories;

namespace PrismaApi.Application.Repositories;

public class IssueRepository : BaseRepository<Issue, Guid>
{
    public readonly IDiscreteTableRuleTrigger _ruleTrigger;
    public IssueRepository(AppDbContext dbContext, IDiscreteTableRuleTrigger ruleTrigger) : base(dbContext)
    {
        _ruleTrigger = ruleTrigger;
    }

    public override async Task UpdateRangeAsync(IEnumerable<Issue> incommingEntities)
    {
        var incomingList = incommingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id));
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

            entity.ProjectId = incomingEntity.ProjectId;
            entity.Type = incomingEntity.Type;
            entity.Boundary = incomingEntity.Boundary;
            entity.Name = incomingEntity.Name;
            entity.Description = incomingEntity.Description;
            entity.Order = incomingEntity.Order;
            entity.UpdatedById = incomingEntity.UpdatedById;

            if (incomingEntity.Node != null && entity.Node != null)
                entity.Node = entity.Node.Update(incomingEntity.Node, DbContext);

            if (incomingEntity.Decision != null && entity.Decision != null)
                entity.Decision = await entity.Decision.Update(incomingEntity.Decision, DbContext, _ruleTrigger);
            
            if (incomingEntity.Uncertainty != null && entity.Uncertainty != null)
                entity.Uncertainty = await entity.Uncertainty.Update(incomingEntity.Uncertainty, DbContext, _ruleTrigger);

            if (incomingEntity.Utility != null && entity.Utility != null)
                entity.Utility = entity.Utility.Update(incomingEntity.Utility, DbContext);
        }
        await _ruleTrigger.ParentIssuesChangedAsync(issuesIdsTriggers);
        await DbContext.SaveChangesAsync();
    }

    public async Task<ICollection<Issue>> GetIssuesInInfluenceDiagram(Guid projectId)
    {
        return await base.GetAllAsync(false, Query().IndluenceDiagramFilter(projectId));
    }

    private bool WillIssueChangeTables(Issue entity, Issue incommingEntity)
    {
        if (entity.Type != incommingEntity.Type) return true;
        if (entity.Boundary != incommingEntity.Boundary && (incommingEntity.Boundary == "out" || entity.Boundary == "out")) return true;
        return false;
    }

    protected override IQueryable<Issue> Query()
    {
        return DbContext.Issues
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
                (e.Boundary == Boundary.In.ToString() || e.Boundary == Boundary.On.ToString()) &&
                (e.Type == IssueType.Uncertainty.ToString() || e.Type == IssueType.Decision.ToString() || e.Type == IssueType.Utility.ToString()) &&
                (
                    (e.Type == IssueType.Uncertainty.ToString() && e.Uncertainty!.IsKey == true) ||
                    (e.Type == IssueType.Decision.ToString() && e.Decision!.Type == "Foucus") ||
                    (e.Type == IssueType.Utility.ToString())
                )
            );
    }
}
