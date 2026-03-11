using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface INodeService
{
     Task<List<NodeOutgoingDto>> UpdateAsync(List<NodeIncomingDto> dtos, UserOutgoingDto userDto);
 Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
 Task<List<NodeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
     Task<List<NodeOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
