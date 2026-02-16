using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class StrategyService
{
    private readonly StrategyRepository _strategyRepository;

    public StrategyService(StrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
    }

    public async Task<List<StrategyOutgoingDto>> CreateAsync(List<StrategyIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _strategyRepository.AddRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var created = await _strategyRepository.GetByIdsAsync(ids);
        return created.ToOutgoingDtos();
    }

    public async Task<List<StrategyOutgoingDto>> UpdateAsync(List<StrategyIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _strategyRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _strategyRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _strategyRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<StrategyOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var strategies = await _strategyRepository.GetByIdsAsync(ids);
        return strategies.ToOutgoingDtos();
    }

    public async Task<List<StrategyOutgoingDto>> GetAllAsync()
    {
        var strategies = await _strategyRepository.GetAllAsync();
        return strategies.ToOutgoingDtos();
    }
}
