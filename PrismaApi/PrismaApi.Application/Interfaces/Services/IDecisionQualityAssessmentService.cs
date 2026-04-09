using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface IDecisionQualityAssessmentService
    {
        Task<DecisionQualityAssessmentOutgoingDto> GetAsync(Guid id, UserOutgoingDto user, CancellationToken ct);
        Task<List<DecisionQualityAssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct);
        Task<List<DecisionQualityAssessmentOutgoingDto>> CreateAsync(List<DecisionQualityAssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct);
        Task UpdateAsync(List<DecisionQualityAssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct);
    }
}
