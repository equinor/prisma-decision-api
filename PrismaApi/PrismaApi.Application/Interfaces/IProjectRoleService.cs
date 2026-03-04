using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IProjectRoleService
{
    Task<List<ProjectRoleOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<ProjectRoleOutgoingDto>> GetAllAsync();
    Task<List<ProjectRoleOutgoingDto>> UpdateAsync(List<ProjectRoleIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids);
}
