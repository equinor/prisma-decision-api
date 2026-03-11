using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IOptionService
{
  Task<List<OptionOutgoingDto>> CreateAsync(List<OptionIncomingDto> dtos);
  Task<List<OptionOutgoingDto>> UpdateAsync(List<OptionIncomingDto> dtos, UserOutgoingDto userDto);
  Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
  Task<List<OptionOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
  Task<List<OptionOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
