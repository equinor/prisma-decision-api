using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IUtilityService
{
    Task<List<UtilityOutgoingDto>> UpdateAsync(List<UtilityIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<UtilityOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<UtilityOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
