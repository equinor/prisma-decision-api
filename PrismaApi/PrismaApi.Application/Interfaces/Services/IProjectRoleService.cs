using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IProjectRoleService
{
    Task<List<ProjectRoleOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<ProjectRoleOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
    Task<List<ProjectRoleOutgoingDto>> UpdateAsync(List<ProjectRoleIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
}
