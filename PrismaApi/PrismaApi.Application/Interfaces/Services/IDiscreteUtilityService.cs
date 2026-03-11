using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDiscreteUtilityService
{
    Task<List<DiscreteUtilityDto>> CreateAsync(List<DiscreteUtilityDto> dtos);
    Task<List<DiscreteUtilityDto>> UpdateAsync(List<DiscreteUtilityDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<DiscreteUtilityDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
  Task<List<DiscreteUtilityDto>> GetAllAsync(UserOutgoingDto user);
}
