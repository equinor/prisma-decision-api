
using System.Linq.Expressions;
using PrismaApi.Domain.Entities;
namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface IStakeholderMatrixRepository : ICrudRepository<StakeholderMatrix, Guid>
    {
        Task UpdateRangeAsync(IEnumerable<StakeholderMatrix> incomingEntities, Expression<Func<StakeholderMatrix, bool>> filterPredicate, CancellationToken ct = default);
    }
}
