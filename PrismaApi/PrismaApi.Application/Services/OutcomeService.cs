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

    public async Task<List<OutcomeOutgoingDto>> UpdateAsync(List<OutcomeIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities();
        await _outcomeRepository.UpdateRangeAsync(entities, UserFilter(userDto));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _outcomeRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _outcomeRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<OutcomeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var entities = await _outcomeRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    public async Task<List<OutcomeOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var entities = await _outcomeRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    private static Expression<Func<Outcome, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Uncertainty!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
