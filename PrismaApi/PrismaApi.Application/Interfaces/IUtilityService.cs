using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IUtilityService
{
    Task<List<UtilityOutgoingDto>> UpdateAsync(List<UtilityIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<UtilityOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<UtilityOutgoingDto>> GetAllAsync();
}
