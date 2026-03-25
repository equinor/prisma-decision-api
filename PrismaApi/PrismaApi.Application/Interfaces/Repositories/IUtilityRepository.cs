using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUtilityRepository : ICrudRepository<Utility, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Utility> incommingEntities, Expression<Func<Utility, bool>> filterPredicate, CancellationToken ct = default);
}
