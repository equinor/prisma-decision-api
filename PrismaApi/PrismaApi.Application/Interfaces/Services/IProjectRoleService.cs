using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IProjectRoleService
{
    Task<List<ProjectRoleOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<ProjectRoleOutgoingDto>> GetAllAsync(UserOutgoingDto user);
    Task<List<ProjectRoleOutgoingDto>> UpdateAsync(List<ProjectRoleIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
}
