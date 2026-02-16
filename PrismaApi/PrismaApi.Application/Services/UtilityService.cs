using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class UtilityService
{
    private readonly UtilityRepository _utilityRepository;

    public UtilityService(UtilityRepository utilityRepository)
    {
        _utilityRepository = utilityRepository;
    }

    public async Task<List<UtilityOutgoingDto>> UpdateAsync(List<UtilityIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _utilityRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _utilityRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _utilityRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<UtilityOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _utilityRepository.GetByIdsAsync(ids);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<UtilityOutgoingDto>> GetAllAsync()
    {
        var entities = await _utilityRepository.GetAllAsync();
        return entities.ToOutgoingDtos();
    }
}
