using System.Linq.Expressions;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectRoleRepository : ICrudRepository<ProjectRole, Guid>
{
    Task UpdateRangeAsync(IEnumerable<ProjectRole> incomingEntities, Expression<Func<ProjectRole, bool>> filterPredicate, CancellationToken ct = default);
    Task<bool> IsUserFacilitatorFromProjectIdsAsync(List<Guid> projectIds, UserOutgoingDto userDto, CancellationToken ct = default);
    Task<bool> IsUserFacilitatorFromRoleIdsAsync(List<Guid> roleIds, UserOutgoingDto userDto, CancellationToken ct = default);
}
