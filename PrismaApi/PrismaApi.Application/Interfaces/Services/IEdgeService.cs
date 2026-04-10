using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IEdgeService
{
    Task<List<EdgeOutgoingDto>> CreateAsync(List<EdgeIncomingDto> dtos, CancellationToken ct = default);
    Task<List<EdgeOutgoingDto>> UpdateAsync(List<EdgeIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<EdgeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<EdgeOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
