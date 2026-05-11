using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IBoardNodeRepository : ICrudRepository<BoardNode, Guid>
{
    Task UpdateRangeAsync(IEnumerable<BoardNode> incomingEntities, Expression<Func<BoardNode, bool>> filterPredicate, CancellationToken ct = default);
}
