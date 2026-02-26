using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableRuleTrigger : IDiscreteTableRuleTrigger
{
    private readonly AppDbContext _db;

    public DiscreteTableRuleTrigger(AppDbContext db) => _db = db;

    public Task ParentOptionsAddedAsync(ICollection<Guid> decisionIds, CancellationToken cancellationToken = default)
        => EnqueueHeadIssuesByParentOptionAsync(decisionIds, cancellationToken);

    public Task ParentOutcomesAddedAsync(ICollection<Guid> uncertaintyIds, CancellationToken cancellationToken = default)
        => EnqueueHeadIssuesByParentOutcomeAsync(uncertaintyIds, cancellationToken);

    public Task ParentIssuesChangedAsync(ICollection<Guid> parentIssueIds, CancellationToken cancellationToken = default)
        => EnqueueHeadIssuesByIssueAsync(parentIssueIds, cancellationToken);

    public void IssuesToBeReset(ICollection<Guid> issueIds)
        => EnqueueIssues(issueIds);

    public Task EdgesDeletedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default)
        => EnqueueHeadIssuesEdgeAsync(edgeIds, cancellationToken);

    public Task EdgesAddedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default)
        => EnqueueHeadIssuesEdgeAsync(edgeIds, cancellationToken);
    
    public Task NodesConnectionsChangeAsync(ICollection<Guid> nodeIds, CancellationToken cancellationToken = default)
        => EnqueueIssuesFromNodeIds(nodeIds, cancellationToken);

    private void EnqueueIssues(ICollection<Guid> issueIds)
    {
        _db.DiscreteTableSessionInfo.EnqueueIssues(issueIds);
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

}