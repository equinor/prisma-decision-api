using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IProjectService
{
    Task<List<ProjectOutgoingDto>> CreateAsync(List<ProjectCreateDto> dtos, bool isProjectDuplicated, UserOutgoingDto userDto, CancellationToken ct = default);
    Task<List<ProjectOutgoingDto>> UpdateAsync(List<ProjectIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<ProjectOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<ProjectOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
    Task<List<PopulatedProjectDto>> GetPopulatedAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<PopulatedProjectDto>> GetAllPopulatedAsync(UserOutgoingDto user, CancellationToken ct = default);
    Task<InfluanceDiagramDto> GetInfluanceDiagramAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
}
