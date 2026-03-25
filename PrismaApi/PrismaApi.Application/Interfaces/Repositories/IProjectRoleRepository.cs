using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRoleRepository : ICrudRepository<ProjectRole, Guid>
{
    Task UpdateRangeAsync(IEnumerable<ProjectRole> incommingEntities, Expression<Func<ProjectRole, bool>> filterPredicate, CancellationToken ct = default);
}
