using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;
public interface IInfluenceDiagramService
{
    Task<InfluenceDiagramDto> GetInfluenceDiagramAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
    Task<InfluenceDiagramDto> GetRestrictedInfluenceDiagramAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
}