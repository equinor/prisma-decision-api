using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class UncertaintyService: IUncertaintyService
{
    private readonly IUncertaintyRepository _uncertaintyRepository;

    public UncertaintyService(IUncertaintyRepository uncertaintyRepository)
    {
        _uncertaintyRepository = uncertaintyRepository;
    }

    public async Task<List<UncertaintyOutgoingDto>> UpdateAsync(List<UncertaintyIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _uncertaintyRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _uncertaintyRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _uncertaintyRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<UncertaintyOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _uncertaintyRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<UncertaintyOutgoingDto>> GetAllAsync()
    {
        var entities = await _uncertaintyRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }
}
