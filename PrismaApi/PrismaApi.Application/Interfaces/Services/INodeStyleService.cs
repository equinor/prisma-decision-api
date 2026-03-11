using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface INodeStyleService
{
 Task<List<NodeStyleOutgoingDto>> UpdateAsync(List<NodeStyleIncomingDto> dtos, UserOutgoingDto userDto);
     Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
     Task<List<NodeStyleOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
     Task<List<NodeStyleOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
