using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRepository : ICrudRepository<Project, Guid>
{
    Task<ICollection<Project>> GetProjectsWhereUserHasAccess(ICollection<Guid> projectIds, int userId);
    Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incommingEntities, Expression<Func<Project, bool>> filterPredicate);
}
