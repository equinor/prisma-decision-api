using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IDiscreteUtilityService
{
    Task<List<DiscreteUtilityDto>> CreateAsync(List<DiscreteUtilityDto> dtos);
    Task<List<DiscreteUtilityDto>> UpdateAsync(List<DiscreteUtilityDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<DiscreteUtilityDto>> GetAsync(List<Guid> ids);
  Task<List<DiscreteUtilityDto>> GetAllAsync();
}
