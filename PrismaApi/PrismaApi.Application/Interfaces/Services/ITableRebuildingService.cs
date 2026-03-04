namespace PrismaApi.Application.Interfaces.Services;

public interface ITableRebuildingService
{
    Task RebuildTablesAsync();
 Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds);
}
