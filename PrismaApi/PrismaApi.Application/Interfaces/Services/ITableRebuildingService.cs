namespace PrismaApi.Application.Interfaces.Services;

public interface ITableRebuildingService
{
    Task RebuildTablesAsync(CancellationToken ct = default);
    Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds, CancellationToken ct = default);
    Task RemoveExcessProbabilities(Guid uncertaintyId, CancellationToken ct = default);
    Task RemoveExcessUtilities(Guid utilityId, CancellationToken ct = default);
}
