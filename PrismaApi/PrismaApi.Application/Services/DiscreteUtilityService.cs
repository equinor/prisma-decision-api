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

public class DiscreteUtilityService : IDiscreteUtilityService
{
    private readonly IDiscreteUtilityRepository _discreteUtilityRepository;

    public DiscreteUtilityService(IDiscreteUtilityRepository discreteUtilityRepository)
    {
        _discreteUtilityRepository = discreteUtilityRepository;
    }

    public async Task<List<DiscreteUtilityDto>> CreateAsync(List<DiscreteUtilityDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _discreteUtilityRepository.AddRangeAsync(entities);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteUtilityDto>> UpdateAsync(List<DiscreteUtilityDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntitiesWithoutParents();
        await _discreteUtilityRepository.UpdateRangeAsync(entities, UserFilter(userDto));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _discreteUtilityRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return updated.ToDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _discreteUtilityRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<DiscreteUtilityDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var entities = await _discreteUtilityRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToDtos();
    }

    public async Task<List<DiscreteUtilityDto>> GetAllAsync(UserOutgoingDto user)
    {
        var entities = await _discreteUtilityRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToDtos();
    }
    private static Expression<Func<DiscreteUtility, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Utility!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
