using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDiscreteProbabilityService
{
    Task<List<DiscreteProbabilityDto>> CreateAsync(List<DiscreteProbabilityDto> dtos);
    Task<List<DiscreteProbabilityDto>> UpdateAsync(List<DiscreteProbabilityDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<DiscreteProbabilityDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<DiscreteProbabilityDto>> GetAllAsync(UserOutgoingDto user);
}
