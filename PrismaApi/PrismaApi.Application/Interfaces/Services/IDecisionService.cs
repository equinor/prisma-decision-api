using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDecisionService
{
    Task<List<DecisionOutgoingDto>> CreateAsync(List<DecisionIncomingDto> dtos);
    Task<List<DecisionOutgoingDto>> UpdateAsync(List<DecisionIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<DecisionOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<DecisionOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
