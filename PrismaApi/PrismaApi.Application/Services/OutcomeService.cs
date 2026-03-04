using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class OutcomeService: IOutcomeService
{
    private readonly IOutcomeRepository _outcomeRepository;

    public OutcomeService(IOutcomeRepository outcomeRepository)
    {
        _outcomeRepository = outcomeRepository;
    }

    public async Task<List<OutcomeOutgoingDto>> CreateAsync(List<OutcomeIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _outcomeRepository.AddRangeAsync(entities);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<OutcomeOutgoingDto>> UpdateAsync(List<OutcomeIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _outcomeRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _outcomeRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _outcomeRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<OutcomeOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _outcomeRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<OutcomeOutgoingDto>> GetAllAsync()
    {
        var entities = await _outcomeRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }
}
