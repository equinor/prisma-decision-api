using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IUncertaintyService
{
    Task<List<UncertaintyOutgoingDto>> UpdateAsync(List<UncertaintyIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<UncertaintyOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<UncertaintyOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
