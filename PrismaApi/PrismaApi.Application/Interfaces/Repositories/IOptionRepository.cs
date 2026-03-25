using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IOptionRepository : ICrudRepository<Option, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Option> incommingEntities, Expression<Func<Option, bool>> filterPredicate, CancellationToken ct = default);
}
