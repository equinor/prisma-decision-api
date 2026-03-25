using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IIssueRepository : ICrudRepository<Issue, Guid>
{
    Task<ICollection<Issue>> GetIssuesInInfluenceDiagram(Guid projectId, Expression<Func<Issue, bool>>? filterPredicate, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<Issue> incomingEntities, Expression<Func<Issue, bool>> filterPredicate, CancellationToken ct = default);
}
