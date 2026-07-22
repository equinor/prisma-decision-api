using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IBoardSheetRepository : ICrudRepository<BoardSheet, Guid>
{
    Task UpdateRangeAsync(IEnumerable<BoardSheet> incomingEntities, Expression<Func<BoardSheet, bool>> filterPredicate, CancellationToken ct = default);
}
