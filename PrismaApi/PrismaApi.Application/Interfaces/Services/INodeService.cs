using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface INodeService
{
    Task<List<NodeOutgoingDto>> UpdateAsync(List<NodeIncomingDto> dtos);
 Task DeleteAsync(List<Guid> ids);
 Task<List<NodeOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<NodeOutgoingDto>> GetAllAsync();
}
