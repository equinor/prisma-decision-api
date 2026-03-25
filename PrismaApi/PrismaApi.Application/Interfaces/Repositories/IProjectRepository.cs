using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRepository : ICrudRepository<Project, Guid>
{
    Task<ICollection<Project>> GetProjectsWhereUserHasAccess(ICollection<Guid> projectIds, string userId, CancellationToken ct = default);
    Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incommingEntities, Expression<Func<Project, bool>> filterPredicate, CancellationToken ct = default);
}
