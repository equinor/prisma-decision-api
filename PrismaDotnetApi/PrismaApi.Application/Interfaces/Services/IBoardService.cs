using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IBoardNodeService
{
    Task<List<BoardNodeOutgoingDto>> CreateAsync(List<BoardNodeIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task<List<BoardNodeOutgoingDto>> UpdateAsync(List<BoardNodeIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<BoardNodeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<BoardNodeOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
