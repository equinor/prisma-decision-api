using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface INodeStyleRepository : ICrudRepository<NodeStyle, Guid>
{
    Task UpdateRangeAsync(IEnumerable<NodeStyle> incomingEntities, Expression<Func<NodeStyle, bool>> filterPredicate, CancellationToken ct = default);
}
