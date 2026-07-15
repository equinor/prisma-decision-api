using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IRestrictionEntryService
{
    Task<List<RestrictionEntryOutgoingDto>> CreateAsync(List<RestrictionEntryIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task<List<RestrictionEntryOutgoingDto>> UpdateAsync(List<RestrictionEntryIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<RestrictionEntryOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<RestrictionEntryOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
