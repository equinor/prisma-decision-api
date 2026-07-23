using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IBoardSheetService
{
    Task<List<BoardSheetOutgoingDto>> CreateAsync(List<BoardSheetIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task<List<BoardSheetOutgoingDto>> UpdateAsync(List<BoardSheetIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<BoardSheetOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<BoardSheetOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
