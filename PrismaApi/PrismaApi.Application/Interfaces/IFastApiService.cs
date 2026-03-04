using System.Net;

namespace PrismaApi.Application.Interfaces;

public interface IFastApiService
{
    Task<FastApiService.ApiResponse> CallDownstreamFastApiGetAsync(string endpoint);
    Task<FastApiService.ApiResponse> CallDownstreamFastApiPostAsync(string endpoint, StringContent content);
}
