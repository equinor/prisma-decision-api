using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IUncertaintyRepository : ICrudRepository<Uncertainty, Guid>
{
    Task UpdateRangeAsync(IEnumerable<Uncertainty> incommingEntities, Expression<Func<Uncertainty, bool>> filterPredicate);
}
