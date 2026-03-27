using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface INodeService
{
    Task<List<NodeOutgoingDto>> UpdateAsync(List<NodeIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<NodeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<NodeOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
