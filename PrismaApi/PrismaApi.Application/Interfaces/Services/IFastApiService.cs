using PrismaApi.Domain.Dtos;
using System.Net;

namespace PrismaApi.Application.Interfaces.Services;

public interface IFastApiService
{
    Task<ApiResponseDto> CallDownstreamFastApiGetAsync(string endpoint);
    Task<ApiResponseDto> CallDownstreamFastApiPostAsync(string endpoint, StringContent content);
    Task<ApiResponseDto> SendInfluenceDiagramToFastApiAsync(Guid projectId, string endpoint);
}
