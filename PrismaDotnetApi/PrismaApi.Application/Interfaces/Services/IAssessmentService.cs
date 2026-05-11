using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<List<AssessmentOutgoingDto>?> CreateAsync(List<AssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<AssessmentOutgoingDto>?> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<AssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
        Task UpdateRangeAsync(List<AssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task DeleteAsync(Guid id, UserOutgoingDto user, CancellationToken ct = default);
    }
}
