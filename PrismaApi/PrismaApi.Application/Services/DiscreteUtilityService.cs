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

public class DiscreteUtilityService: IDiscreteUtilityService
{
    private readonly IDiscreteUtilityRepository _discreteUtilityRepository;

    public DiscreteUtilityService(IDiscreteUtilityRepository discreteUtilityRepository)
    {
        _discreteUtilityRepository = discreteUtilityRepository;
    }

    public async Task<List<DiscreteUtilityDto>> CreateAsync(List<DiscreteUtilityDto> dtos)
    {
        var entities = dtos.ToEntitiesWithoutParents();
        await _discreteUtilityRepository.AddRangeAsync(entities);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteUtilityDto>> UpdateAsync(List<DiscreteUtilityDto> dtos)
    {
        var entities = dtos.ToEntitiesWithoutParents();
        await _discreteUtilityRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _discreteUtilityRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _discreteUtilityRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<DiscreteUtilityDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _discreteUtilityRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteUtilityDto>> GetAllAsync()
    {
        var entities = await _discreteUtilityRepository.GetAllAsync(withTracking: false);
        return entities.ToDtos();
    }
    private static Expression<Func<DiscreteUtility, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Utility!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
