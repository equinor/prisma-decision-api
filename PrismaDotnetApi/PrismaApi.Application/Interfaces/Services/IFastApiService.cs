using PrismaApi.Domain.Dtos;
using System.Net;

namespace PrismaApi.Application.Interfaces.Services;

public interface IFastApiService
{
    Task<ApiResponseDto> CallDownstreamFastApiGetAsync(string endpoint, CancellationToken ct = default);
    Task<ApiResponseDto> CallDownstreamFastApiPostAsync(string endpoint, StringContent content, CancellationToken ct = default);
    Task<ApiResponseDto> SendInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, UserOutgoingDto user, CancellationToken ct = default);
    Task<ApiResponseDto> SendPartialInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, List<List<Guid>> paths, UserOutgoingDto user, CancellationToken ct = default);
    Task<ApiResponseDto> SendInfluenceDiagramWithEvidenceToFastApiAsync(Guid projectId, string endpoint, List<EvidenceRequestDto> data, UserOutgoingDto user, CancellationToken ct = default);
    Task<ApiResponseDto> SendInfluenceDiagramPolicyTableToFastApiAsync(Guid projectId, string endpoint, EvidenceRequestDto? evidence, UserOutgoingDto user, CancellationToken ct = default);
    List<PolicyTableOutgoingDto> ParsePolicyTableResponse(string? content);
}
