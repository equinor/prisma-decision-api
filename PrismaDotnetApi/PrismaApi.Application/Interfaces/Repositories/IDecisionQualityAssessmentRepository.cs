using System.Linq.Expressions;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Interfaces.Repositories
{
    public interface IDecisionQualityAssessmentRepository : ICrudRepository<DecisionQualityAssessment, Guid>

    {
        Task UpdateRangeAsync(List<DecisionQualityAssessment> incomingEntities, Expression<Func<DecisionQualityAssessment, bool>> filterPredicate, CancellationToken ct = default);
    }
}
