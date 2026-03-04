using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IStrategyService
{
    Task<List<StrategyOutgoingDto>> CreateAsync(List<StrategyIncomingDto> dtos, UserOutgoingDto userDto);
    Task<List<StrategyOutgoingDto>> UpdateAsync(List<StrategyIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids);
    Task<List<StrategyOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<StrategyOutgoingDto>> GetAllAsync();
}
