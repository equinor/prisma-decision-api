using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IOutcomeService
{
    Task<List<OutcomeOutgoingDto>> CreateAsync(List<OutcomeIncomingDto> dtos);
    Task<List<OutcomeOutgoingDto>> UpdateAsync(List<OutcomeIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<OutcomeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<OutcomeOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
