namespace PrismaApi.Application.Interfaces.Services;

public interface ITableRebuildingService
{
    Task RebuildTablesAsync(CancellationToken ct = default);
    Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds, CancellationToken ct = default);
    Task RemoveExcessDiscreteProbabilities(Guid uncertaintyId, CancellationToken ct = default);
    Task RemoveExcessDiscreteUtilities(Guid utilityId, CancellationToken ct = default);
}
