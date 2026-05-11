using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUncertaintyRepository : ICrudRepository<Uncertainty, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Uncertainty> incomingEntities, Expression<Func<Uncertainty, bool>> filterPredicate, CancellationToken ct = default);
}
