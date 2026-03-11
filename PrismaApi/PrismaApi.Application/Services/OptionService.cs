using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PrismaApi.Application.Services;

public class OptionService: IOptionService
{
    private readonly IOptionRepository _optionRepository;

    public OptionService(IOptionRepository optionRepository)
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
        var updated = await _optionRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _optionRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<OptionOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _optionRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<OptionOutgoingDto>> GetAllAsync()
    {
        var entities = await _optionRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }

    private static Expression<Func<Option, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Decision!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
