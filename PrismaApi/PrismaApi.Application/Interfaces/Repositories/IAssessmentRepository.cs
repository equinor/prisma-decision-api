using System.Linq.Expressions;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface IAssessmentRepository : ICrudRepository<Assessment, Guid>
    {
        Task UpdateAsync(List<Assessment> incomingEntities, Expression<Func<Assessment, bool>> filterPredicate, CancellationToken ct = default);

    }
}
