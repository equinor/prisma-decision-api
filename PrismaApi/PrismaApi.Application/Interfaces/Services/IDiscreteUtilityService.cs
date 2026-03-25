using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDiscreteUtilityService
{
    Task<List<DiscreteUtilityDto>> CreateAsync(List<DiscreteUtilityDto> dtos, CancellationToken ct = default);
    Task<List<DiscreteUtilityDto>> UpdateAsync(List<DiscreteUtilityDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<DiscreteUtilityDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<DiscreteUtilityDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
