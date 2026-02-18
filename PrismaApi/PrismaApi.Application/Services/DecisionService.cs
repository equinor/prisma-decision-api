using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class DecisionService
{
    private readonly DecisionRepository _decisionRepository;

    public DecisionService(DecisionRepository decisionRepository)
    {
        _decisionRepository = decisionRepository;
    }

    public async Task<List<DecisionOutgoingDto>> CreateAsync(List<DecisionIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _decisionRepository.AddRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var created = await _decisionRepository.GetByIdsAsync(ids, withTracking: false);
        return created.ToOutgoingDtos();
    }

    public async Task<List<DecisionOutgoingDto>> UpdateAsync(List<DecisionIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _decisionRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _decisionRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _decisionRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<DecisionOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _decisionRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<DecisionOutgoingDto>> GetAllAsync()
    {
        var entities = await _decisionRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }
}
