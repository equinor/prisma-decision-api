using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IDecisionRepository : ICrudRepository<Decision, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Decision> incomingEntities, Expression<Func<Decision, bool>> filterPredicate);
}
