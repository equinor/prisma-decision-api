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

public class UtilityService: IUtilityService
{
    private readonly IUtilityRepository _utilityRepository;

    public UtilityService(IUtilityRepository utilityRepository)
    {
        _utilityRepository = utilityRepository;
    }

    public async Task<List<UtilityOutgoingDto>> UpdateAsync(List<UtilityIncomingDto> dtos, UserOutgoingDto user)
    {
        var entities = dtos.ToEntities();
        await _utilityRepository.UpdateRangeAsync(entities, UserFilter(user));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _utilityRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _utilityRepository.DeleteByIdsAsync(ids, UserFilter(user));
    }

    public async Task<List<UtilityOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var entities = await _utilityRepository.GetByIdsAsync(ids, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    public async Task<List<UtilityOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var entities = await _utilityRepository.GetAllAsync(filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }
    private static Expression<Func<Utility, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
