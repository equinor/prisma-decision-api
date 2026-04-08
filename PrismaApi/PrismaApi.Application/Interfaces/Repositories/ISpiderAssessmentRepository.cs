using System.Linq.Expressions;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface ISpiderAssessmentRepository : ICrudRepository<SpiderAssessment, Guid>

    {
        Task UpdateRangeAsync(List<SpiderAssessment> incomingEntities, Expression<Func<SpiderAssessment, bool>> filterPredicate, CancellationToken ct);
    }
}
