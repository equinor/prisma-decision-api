using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class DiscreteProbabilityService: IDiscreteProbabilityService
{
    private readonly DiscreteProbabilityRepository _discreteProbabilityRepository;

    public DiscreteProbabilityService(DiscreteProbabilityRepository discreteProbabilityRepository)
    {
        _discreteProbabilityRepository = discreteProbabilityRepository;
    }

    public async Task<List<DiscreteProbabilityDto>> CreateAsync(List<DiscreteProbabilityDto> dtos)
    {
        var entities = dtos.ToEntitiesWithoutParents();
        await _discreteProbabilityRepository.AddRangeAsync(entities);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteProbabilityDto>> UpdateAsync(List<DiscreteProbabilityDto> dtos)
    {
        var entities = dtos.ToEntitiesWithoutParents();
        await _discreteProbabilityRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _discreteProbabilityRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _discreteProbabilityRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<DiscreteProbabilityDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _discreteProbabilityRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteProbabilityDto>> GetAllAsync()
    {
        var entities = await _discreteProbabilityRepository.GetAllAsync(withTracking: false);
        return entities.ToDtos();
    }
}
