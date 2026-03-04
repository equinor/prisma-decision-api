using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDiscreteProbabilityService
{
    Task<List<DiscreteProbabilityDto>> CreateAsync(List<DiscreteProbabilityDto> dtos);
    Task<List<DiscreteProbabilityDto>> UpdateAsync(List<DiscreteProbabilityDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<DiscreteProbabilityDto>> GetAsync(List<Guid> ids);
    Task<List<DiscreteProbabilityDto>> GetAllAsync();
}
