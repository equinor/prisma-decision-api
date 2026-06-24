using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrismaApi.Domain.Constants;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;
using System.Linq;
using System.Threading;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableRuleEventHandler : IDiscreteTableRuleEventHandler
{
    private readonly AppDbContext _db;

    public DiscreteTableRuleEventHandler(AppDbContext db) => _db = db;

    public async Task OnDecisionOptionsAddedAsync(ICollection<Guid> decisionIds, CancellationToken cancellationToken = default)
    {
        await EnqueueHeadIssuesByParentOptionAsync(decisionIds, cancellationToken);
        await EnqueueRestrictionTablesByDecisionAsync(decisionIds, cancellationToken);
    }

    public async Task OnUncertaintyOutcomesAddedAsync(ICollection<Guid> uncertaintyIds, CancellationToken cancellationToken = default)
    {
        await EnqueueHeadIssuesByParentOutcomeAsync(uncertaintyIds, cancellationToken);
        await EnqueueRestrictionTablesByUncertaintyAsync(uncertaintyIds, cancellationToken);
    }

    public async Task ParentIssuesChangedAsync(ICollection<Guid> parentIssueIds, CancellationToken cancellationToken = default)
    {
        await EnqueueHeadIssuesByIssueAsync(parentIssueIds, cancellationToken);
        await EnqueueRestrictionTablesByIssueAsync(parentIssueIds, cancellationToken);
    }

    public void EnqueueIssuesForRebuild(ICollection<Guid> issueIds)
        => EnqueueIssues(issueIds);

    public void EnqueueRestrictionTablesForRebuild(ICollection<Guid> restrictionTableIds)
        => EnqueueRestrictionTables(restrictionTableIds);

    public async Task OnEdgesRemovedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default)
    {
        await EnqueueHeadIssuesEdgeAsync(edgeIds, cancellationToken);
        await EnqueueRestrictionTablesByEdgeAsync(edgeIds, cancellationToken);
    }

    public async Task OnEdgesCreatedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default)
    {
        await EnqueueHeadIssuesEdgeAsync(edgeIds, cancellationToken);
    }

    public async Task OnNodeConnectionsChangedAsync(ICollection<Guid> nodeIds, CancellationToken cancellationToken = default)
    {
        await EnqueueIssuesFromNodeIds(nodeIds, cancellationToken);
        await EnqueueRestrictionTablesByNodeAsync(nodeIds, cancellationToken);
    }

    private void EnqueueIssues(ICollection<Guid> issueIds)
    {
        _db.DiscreteTableSessionInfo.EnqueueIssues(issueIds);
    }

    private void EnqueueRestrictionTables(ICollection<Guid> restrictionTableIds)
    {
        _db.DiscreteTableSessionInfo.EnqueueRestrictionTables(restrictionTableIds);
    }

    private async Task EnqueueHeadIssuesByParentOptionAsync(ICollection<Guid> decisionIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;
        var headIssueIds = await _db.Edges
            .AsNoTracking()
            .Include(e => e.HeadNode)
            .Where(e => decisionIds.Contains(e.TailNode!.Issue!.Decision!.Id)) // all the edges that has this as a tail
            .Select(e => e.HeadNode!.IssueId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var issueIds = await _db.Decisions
            .AsNoTracking()
            .Where(e => decisionIds.Contains(e.Id))
            .Select(e => e.IssueId)
            .Distinct()
            .ToListAsync (cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueIssues(headIssueIds);
        _db.DiscreteTableSessionInfo.EnqueueIssues(issueIds);
    }

    private async Task EnqueueHeadIssuesByParentOutcomeAsync(ICollection<Guid> uncertaintyIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;
        var headIssueIds = await _db.Edges
            .AsNoTracking()
            .Include(e => e.HeadNode)
            .Where(e => uncertaintyIds.Contains(e.TailNode!.Issue!.Uncertainty!.Id)) // all the edges that has this as a tail
            .Select(e => e.HeadNode!.IssueId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var issueIds = await _db.Uncertainties
            .AsNoTracking()
            .Where(e => uncertaintyIds.Contains(e.Id))
            .Select(e => e.IssueId)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueIssues(headIssueIds);
        _db.DiscreteTableSessionInfo.EnqueueIssues(issueIds);
    }

    private async Task EnqueueHeadIssuesByIssueAsync(ICollection<Guid> issueIds, CancellationToken cancellationToken = default)
    {

        if (_db.IsDiscreteTableEventDisabled)
            return;
        var headIssueIds = await _db.Edges
            .AsNoTracking()
            .Include(e => e.HeadNode)
            .Where(e => issueIds.Contains(e.TailNode!.IssueId))
            .Select(e => e.HeadNode!.IssueId)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueIssues(headIssueIds);
    }

    private async Task EnqueueHeadIssuesEdgeAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var headIssueIds = await _db.Edges
            .AsNoTracking()
            .Include(e => e.HeadNode)
            .Where(e => edgeIds.Contains(e.Id))
            .Select(e => e.HeadNode!.IssueId)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueIssues(headIssueIds);
    }
    private async Task EnqueueIssuesFromNodeIds(ICollection<Guid> nodeIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var issueIds = await _db.Nodes
            .AsNoTracking()
            .Where(e => nodeIds.Contains(e.Id))
            .Select(e => e.IssueId)
            .Distinct()
            .ToListAsync();

        _db.DiscreteTableSessionInfo.EnqueueIssues(issueIds);
    }

    private async Task EnqueueRestrictionTablesByDecisionAsync(ICollection<Guid> decisionIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var restrictionTableIds = await _db.RestrictionTables
            .AsNoTracking()
            .Where(rt =>
                decisionIds.Contains(rt.Edge!.TailNode!.Issue!.Decision!.Id) ||
                decisionIds.Contains(rt.Edge!.HeadNode!.Issue!.Decision!.Id))
            .Select(rt => rt.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueRestrictionTables(restrictionTableIds);
    }

    private async Task EnqueueRestrictionTablesByUncertaintyAsync(ICollection<Guid> uncertaintyIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var restrictionTableIds = await _db.RestrictionTables
            .AsNoTracking()
            .Where(rt =>
                uncertaintyIds.Contains(rt.Edge!.TailNode!.Issue!.Uncertainty!.Id) ||
                uncertaintyIds.Contains(rt.Edge!.HeadNode!.Issue!.Uncertainty!.Id))
            .Select(rt => rt.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueRestrictionTables(restrictionTableIds);
    }

    private async Task EnqueueRestrictionTablesByEdgeAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var restrictionTableIds = await _db.RestrictionTables
            .AsNoTracking()
            .Where(rt => edgeIds.Contains(rt.EdgeId))
            .Select(rt => rt.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueRestrictionTables(restrictionTableIds);
    }

    private async Task EnqueueRestrictionTablesByIssueAsync(ICollection<Guid> issueIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var restrictionTableIds = await _db.RestrictionTables
            .AsNoTracking()
            .Where(rt =>
                issueIds.Contains(rt.Edge!.HeadNode!.IssueId) ||
                issueIds.Contains(rt.Edge!.TailNode!.IssueId))
            .Select(rt => rt.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueRestrictionTables(restrictionTableIds);
    }

    private async Task EnqueueRestrictionTablesByNodeAsync(ICollection<Guid> nodeIds, CancellationToken cancellationToken = default)
    {
        if (_db.IsDiscreteTableEventDisabled)
            return;

        var restrictionTableIds = await _db.RestrictionTables
            .AsNoTracking()
            .Where(rt =>
                nodeIds.Contains(rt.Edge!.HeadId) ||
                nodeIds.Contains(rt.Edge!.TailId))
            .Select(rt => rt.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        _db.DiscreteTableSessionInfo.EnqueueRestrictionTables(restrictionTableIds);
    }

}
