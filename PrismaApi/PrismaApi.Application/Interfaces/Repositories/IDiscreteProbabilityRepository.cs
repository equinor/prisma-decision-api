using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IDiscreteProbabilityRepository : ICrudRepository<DiscreteProbability, Guid>
{
    Task UpdateRangeAsync(IEnumerable<DiscreteProbability> incomingEntities, Expression<Func<DiscreteProbability, bool>> filterPredicate, CancellationToken ct = default);
}
