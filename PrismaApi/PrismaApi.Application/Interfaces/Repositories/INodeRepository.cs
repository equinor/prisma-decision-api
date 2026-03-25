using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface INodeRepository : ICrudRepository<Node, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Node> incomingEntities, Expression<Func<Node, bool>> filterPredicate);
}
