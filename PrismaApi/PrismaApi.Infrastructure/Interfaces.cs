namespace PrismaApi.Infrastructure;

public interface IDiscreteTableRuleTrigger
{
    Task ParentOptionsAddedAsync(ICollection<Guid> optionIds, CancellationToken cancellationToken = default);
    Task ParentOutcomesAddedAsync(ICollection<Guid> outcomeIds, CancellationToken cancellationToken = default);
    Task ParentIssuesChangedAsync(ICollection<Guid> issueIds, CancellationToken cancellationToken = default);
    void IssuesToBeReset(ICollection<Guid> issueIds);
    Task EdgesDeletedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default);
    Task EdgesAddedAsync(ICollection<Guid> edgeIds, CancellationToken cancellationToken = default);
    Task NodesConnectionsChangeAsync(ICollection<Guid> nodeIds, CancellationToken cancellationToken = default);

}
