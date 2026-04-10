using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IOptionService
{
    Task<List<OptionOutgoingDto>> CreateAsync(List<OptionIncomingDto> dtos, CancellationToken ct = default);
    Task<List<OptionOutgoingDto>> UpdateAsync(List<OptionIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<OptionOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
    Task<List<OptionOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
}
