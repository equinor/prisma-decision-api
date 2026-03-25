using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IOutcomeRepository : ICrudRepository<Outcome, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Outcome> incommingEntities, Expression<Func<Outcome, bool>> filterPredicate, CancellationToken ct = default);
}
