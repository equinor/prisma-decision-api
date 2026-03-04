using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface INodeStyleService
{
 Task<List<NodeStyleOutgoingDto>> UpdateAsync(List<NodeStyleIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<NodeStyleOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<NodeStyleOutgoingDto>> GetAllAsync();
}
