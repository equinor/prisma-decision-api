using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDiscreteProbabilityService
{
    Task<List<DiscreteProbabilityDto>> CreateAsync(List<DiscreteProbabilityDto> dtos, CancellationToken ct = default);
    Task<List<DiscreteProbabilityDto>> UpdateAsync(List<DiscreteProbabilityDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<DiscreteProbabilityDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<DiscreteProbabilityDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
