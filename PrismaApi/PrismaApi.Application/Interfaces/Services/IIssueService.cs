using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IIssueService
{
    Task<List<IssueOutgoingDto>> CreateAsync(List<IssueIncomingDto> dtos, UserOutgoingDto userDto);
Task<List<IssueOutgoingDto>> UpdateAsync(List<IssueIncomingDto> dtos, UserOutgoingDto userDto);
    Task DeleteAsync(List<Guid> ids);
    Task<List<IssueOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<IssueOutgoingDto>> GetAllAsync();
}
