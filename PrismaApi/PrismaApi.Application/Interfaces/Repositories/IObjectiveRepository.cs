using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IObjectiveRepository : ICrudRepository<Objective, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Objective> incomingEntities, Expression<Func<Objective, bool>> filterPredicate, CancellationToken ct = default);
}
