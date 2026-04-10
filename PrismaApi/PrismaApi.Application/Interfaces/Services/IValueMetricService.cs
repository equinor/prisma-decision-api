using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IValueMetricService
{
    Task<List<ValueMetricOutgoingDto>> UpdateAsync(List<ValueMetricIncomingDto> dtos);
    Task DeleteAsync(List<Guid> ids);
    Task<List<ValueMetricOutgoingDto>> GetAsync(List<Guid> ids);
    Task<List<ValueMetricOutgoingDto>> GetAllAsync();
}
