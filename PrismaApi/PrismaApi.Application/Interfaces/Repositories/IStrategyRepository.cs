using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IStrategyRepository : ICrudRepository<Strategy, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Strategy> incomingEntities, Expression<Func<Strategy, bool>> filterPredicate, CancellationToken ct = default);
}
