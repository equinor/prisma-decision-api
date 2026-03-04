using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IEdgeService
{
    Task<List<EdgeOutgoingDto>> CreateAsync(List<EdgeIncomingDto> dtos);
    Task<List<EdgeOutgoingDto>> UpdateAsync(List<EdgeIncomingDto> dtos);
 Task DeleteAsync(List<Guid> ids);
 Task<List<EdgeOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<EdgeOutgoingDto>> GetAllAsync();
}
