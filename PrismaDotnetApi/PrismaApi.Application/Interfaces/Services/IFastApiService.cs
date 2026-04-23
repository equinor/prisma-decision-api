using PrismaApi.Domain.Dtos;
using System.Net;

namespace PrismaApi.Application.Interfaces.Services;

public interface IFastApiService
{
    Task<ApiResponseDto> CallDownstreamFastApiGetAsync(string endpoint, CancellationToken ct = default);
    Task<ApiResponseDto> CallDownstreamFastApiPostAsync(string endpoint, StringContent content, CancellationToken ct = default);
    Task<ApiResponseDto> SendInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint, UserOutgoingDto user, CancellationToken ct = default);
}
