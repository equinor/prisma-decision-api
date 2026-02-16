using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class OptionService
{
    private readonly OptionRepository _optionRepository;

    public OptionService(OptionRepository optionRepository)
    {
        _optionRepository = optionRepository;
    }

    public async Task<List<OptionOutgoingDto>> CreateAsync(List<OptionIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _optionRepository.AddRangeAsync(entities);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<OptionOutgoingDto>> UpdateAsync(List<OptionIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _optionRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _optionRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _optionRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<OptionOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _optionRepository.GetByIdsAsync(ids);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<OptionOutgoingDto>> GetAllAsync()
    {
        var entities = await _optionRepository.GetAllAsync();
        return entities.ToOutgoingDtos();
    }
}
