using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IProjectDuplicationService
{
    Task<ProjectOutgoingDto> DuplicateAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
    Task<ProjectOutgoingDto?> DuplicateImportedProjectAsync(ProjectImportDto dto, UserOutgoingDto user, CancellationToken ct = default);
}
