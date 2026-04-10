using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IOutcomeService
{
    Task<List<OutcomeOutgoingDto>> CreateAsync(List<OutcomeIncomingDto> dtos, CancellationToken ct = default);
    Task<List<OutcomeOutgoingDto>> UpdateAsync(List<OutcomeIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<OutcomeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<OutcomeOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
