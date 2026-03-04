namespace PrismaApi.Infrastructure;

public interface IDiscreteTableRuleEventHandler
{
    Task OnDecisionOptionsAddedAsync(ICollection<Guid> decisionIds, CancellationToken cancellationToken = default);
    Task OnUncertaintyOutcomesAddedAsync(ICollection<Guid> uncertaintyIds, CancellationToken cancellationToken = default);
    Task ParentIssuesChangedAsync(ICollection<Guid> issueIds, CancellationToken cancellationToken = default);
    void EnqueueIssuesForRebuild(ICollection<Guid> issueIds);
    Task OnEdgesRemovedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default);
    Task OnEdgesCreatedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default);
    Task OnNodeConnectionsChangedAsync(ICollection<Guid> nodeIds, CancellationToken cancellationToken = default);

}
