using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IUncertaintyService
{
    Task<List<UncertaintyOutgoingDto>> UpdateAsync(List<UncertaintyIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<UncertaintyOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<UncertaintyOutgoingDto>> GetAllAsync();
}
