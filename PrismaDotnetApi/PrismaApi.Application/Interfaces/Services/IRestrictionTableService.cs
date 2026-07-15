using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IRestrictionTableService
{
    Task<List<RestrictionTableOutgoingDto>> CreateAsync(List<RestrictionTableIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task<List<RestrictionTableOutgoingDto>> UpdateAsync(List<RestrictionTableIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<RestrictionTableOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<RestrictionTableOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
