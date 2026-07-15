using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IRestrictionTableRepository : ICrudRepository<RestrictionTable, Guid>
{
    Task UpdateRangeAsync(IEnumerable<RestrictionTable> incomingEntities, Expression<Func<RestrictionTable, bool>> filterPredicate, CancellationToken ct = default);
}
