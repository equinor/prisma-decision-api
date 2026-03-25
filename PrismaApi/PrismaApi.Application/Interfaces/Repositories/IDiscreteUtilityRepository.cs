using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IDiscreteUtilityRepository : ICrudRepository<DiscreteUtility, Guid>
{
    Task UpdateRangeAsync(IEnumerable<DiscreteUtility> incomingEntities, Expression<Func<DiscreteUtility, bool>> filterPredicate, CancellationToken ct = default);
}
