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

public class StrategyService: IStrategyService
{
    private readonly IStrategyRepository _strategyRepository;

    public StrategyService(IStrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
    }

    public async Task<List<StrategyOutgoingDto>> CreateAsync(List<StrategyIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities(userDto);
        await _strategyRepository.AddRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var created = await _strategyRepository.GetByIdsAsync(ids);
        return created.ToOutgoingDtos();
    }

    public async Task<List<StrategyOutgoingDto>> UpdateAsync(List<StrategyIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities(userDto);
        await _strategyRepository.UpdateRangeAsync(entities, UserFilter(userDto));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _strategyRepository.GetByIdsAsync(ids, filterPredicate: UserFilter(userDto));
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _strategyRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<StrategyOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var strategies = await _strategyRepository.GetByIdsAsync(ids, filterPredicate: UserFilter(user));
        return strategies.ToOutgoingDtos();
    }

    public async Task<List<StrategyOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var strategies = await _strategyRepository.GetAllAsync(filterPredicate: UserFilter(user));
        return strategies.ToOutgoingDtos();
    }

    private static Expression<Func<Strategy, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
