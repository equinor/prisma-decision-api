using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class ValueMetricService
{
    private readonly ValueMetricRepository _valueMetricRepository;

    public ValueMetricService(ValueMetricRepository valueMetricRepository)
    {
        _valueMetricRepository = valueMetricRepository;
    }

    public async Task<List<ValueMetricOutgoingDto>> UpdateAsync(List<ValueMetricIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _valueMetricRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _valueMetricRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _valueMetricRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<ValueMetricOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _valueMetricRepository.GetByIdsAsync(ids);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<ValueMetricOutgoingDto>> GetAllAsync()
    {
        var entities = await _valueMetricRepository.GetAllAsync();
        return entities.ToOutgoingDtos();
    }
}
