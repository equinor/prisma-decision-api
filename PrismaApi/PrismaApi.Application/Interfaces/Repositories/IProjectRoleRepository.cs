using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRoleRepository : ICrudRepository<ProjectRole, Guid>
{
    Task UpdateRangeAsync(IEnumerable<ProjectRole> incomingEntities, Expression<Func<ProjectRole, bool>> filterPredicate);
}
