using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IDecisionService
{
    Task<List<DecisionOutgoingDto>> CreateAsync(List<DecisionIncomingDto> dtos, CancellationToken ct = default);
    Task<List<DecisionOutgoingDto>> UpdateAsync(List<DecisionIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<DecisionOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<DecisionOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
