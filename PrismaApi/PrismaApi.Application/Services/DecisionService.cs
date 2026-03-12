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

public class DecisionService: IDecisionService
{
    private readonly IDecisionRepository _decisionRepository;

    public DecisionService(IDecisionRepository decisionRepository)
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

    public async Task<List<DecisionOutgoingDto>> UpdateAsync(List<DecisionIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities();
        await _decisionRepository.UpdateRangeAsync(entities, UserFilter(userDto));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _decisionRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _decisionRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<DecisionOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var entities = await _decisionRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    public async Task<List<DecisionOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var entities = await _decisionRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    private static Expression<Func<Decision, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
