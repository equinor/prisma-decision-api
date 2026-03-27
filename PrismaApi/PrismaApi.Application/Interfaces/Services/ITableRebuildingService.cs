namespace PrismaApi.Application.Interfaces.Services;

public interface ITableRebuildingService
{
    Task RebuildTablesAsync(CancellationToken ct = default);
    Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds, CancellationToken ct = default);
}
