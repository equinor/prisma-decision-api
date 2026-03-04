namespace PrismaApi.Application.Interfaces;

public interface ITableRebuildingService
{
    Task RebuildTablesAsync();
 Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds);
}
