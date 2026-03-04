using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IDecisionService
{
    Task<List<DecisionOutgoingDto>> CreateAsync(List<DecisionIncomingDto> dtos);
    Task<List<DecisionOutgoingDto>> UpdateAsync(List<DecisionIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<DecisionOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<DecisionOutgoingDto>> GetAllAsync();
}
