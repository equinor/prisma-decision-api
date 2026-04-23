using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Repositories;

public interface IProjectDuplicationRepository
{
    Task<FullProjectForDuplicationDto?> GetFullProjectForDuplicationAsync(
        Guid projectId,
        UserOutgoingDto user,
        CancellationToken ct = default);
}
