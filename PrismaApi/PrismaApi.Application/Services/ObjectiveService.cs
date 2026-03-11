using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Services;

public class ObjectiveService: IObjectiveService
{
    private readonly IObjectiveRepository _objectiveRepository;

    public ObjectiveService(IObjectiveRepository objectiveRepository)
    {
        _objectiveRepository = objectiveRepository;
    }

    public async Task<List<ObjectiveOutgoingDto>> CreateAsync(List<ObjectiveIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities(userDto);
        await _objectiveRepository.AddRangeAsync(entities);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<ObjectiveOutgoingDto>> UpdateAsync(List<ObjectiveIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities(userDto);
        await _objectiveRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _objectiveRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _objectiveRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<ObjectiveOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var entities = await _objectiveRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    public async Task<List<ObjectiveOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var entities = await _objectiveRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    private static Expression<Func<Objective, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
