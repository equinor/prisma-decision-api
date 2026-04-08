using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface ISpiderAssessmentService
    {
        Task<SpiderAssessmentOutgoingDto> GetAsync(Guid id, UserOutgoingDto user, CancellationToken ct);
        Task<List<SpiderAssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct);
        Task<List<SpiderAssessmentOutgoingDto>> CreateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct);
        Task UpdateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct);
    }
}
