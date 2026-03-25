using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRepository : ICrudRepository<Project, Guid>
{
    Task<ICollection<Project>> GetProjectsWhereUserHasAccess(ICollection<Guid> projectIds, string userId);
    Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incomingEntities, Expression<Func<Project, bool>> filterPredicate);
}
