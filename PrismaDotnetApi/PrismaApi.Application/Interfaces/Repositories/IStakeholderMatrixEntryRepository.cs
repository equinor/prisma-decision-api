
using System.Linq.Expressions;
using PrismaApi.Domain.Entities;
namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface IStakeholderMatrixEntryRepository : ICrudRepository<StakeholderMatrixEntry, Guid>
    {
        Task UpdateRangeAsync(IEnumerable<StakeholderMatrixEntry> incomingEntities, Expression<Func<StakeholderMatrixEntry, bool>> filterPredicate, CancellationToken ct = default);
    }
}
