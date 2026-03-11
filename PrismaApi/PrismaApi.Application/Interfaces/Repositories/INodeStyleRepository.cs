using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface INodeStyleRepository : ICrudRepository<NodeStyle, Guid>
{
    Task UpdateRangeAsync(IEnumerable<NodeStyle> incommingEntities, Expression<Func<NodeStyle, bool>> filterPredicate);
}
