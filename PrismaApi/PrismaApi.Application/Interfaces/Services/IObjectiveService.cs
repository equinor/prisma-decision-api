using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IObjectiveService
{
    Task<List<ObjectiveOutgoingDto>> CreateAsync(List<ObjectiveIncomingDto> dtos, UserOutgoingDto userDto);
    Task<List<ObjectiveOutgoingDto>> UpdateAsync(List<ObjectiveIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
  Task<List<ObjectiveOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user);
    Task<List<ObjectiveOutgoingDto>> GetAllAsync(UserOutgoingDto user);
}
