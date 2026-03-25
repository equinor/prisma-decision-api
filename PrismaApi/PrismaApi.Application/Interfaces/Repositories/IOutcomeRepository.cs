using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IOutcomeRepository : ICrudRepository<Outcome, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Outcome> incomingEntities, Expression<Func<Outcome, bool>> filterPredicate);
}
