using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IOutcomeService
{
    Task<List<OutcomeOutgoingDto>> CreateAsync(List<OutcomeIncomingDto> dtos);
    Task<List<OutcomeOutgoingDto>> UpdateAsync(List<OutcomeIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<OutcomeOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<OutcomeOutgoingDto>> GetAllAsync();
}
