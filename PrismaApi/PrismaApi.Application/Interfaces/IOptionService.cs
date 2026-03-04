using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces;

public interface IOptionService
{
  Task<List<OptionOutgoingDto>> CreateAsync(List<OptionIncomingDto> dtos);
    Task<List<OptionOutgoingDto>> UpdateAsync(List<OptionIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<OptionOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<OptionOutgoingDto>> GetAllAsync();
}
