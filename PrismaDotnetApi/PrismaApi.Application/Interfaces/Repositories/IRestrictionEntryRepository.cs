using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IRestrictionEntryRepository : ICrudRepository<RestrictionEntry, Guid>
{
    Task UpdateRangeAsync(IEnumerable<RestrictionEntry> incomingEntities, Expression<Func<RestrictionEntry, bool>> filterPredicate, CancellationToken ct = default);
}
