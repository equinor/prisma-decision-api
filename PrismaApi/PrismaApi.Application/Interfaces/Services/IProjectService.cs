using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IProjectService
{
    Task<List<ProjectOutgoingDto>> CreateAsync(List<ProjectCreateDto> dtos, UserOutgoingDto userDto);
    Task<List<ProjectOutgoingDto>> UpdateAsync(List<ProjectIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<ProjectOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<ProjectOutgoingDto>> GetAllAsync(UserOutgoingDto user);
    Task<List<PopulatedProjectDto>> GetPopulatedAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<PopulatedProjectDto>> GetAllPopulatedAsync(UserOutgoingDto user);
    Task<InfluanceDiagramDto> GetInfluanceDiagramAsync(Guid projectId, UserOutgoingDto user);
}
