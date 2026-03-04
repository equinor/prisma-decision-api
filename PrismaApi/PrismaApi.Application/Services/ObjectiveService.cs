using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

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
        var updated = await _objectiveRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _objectiveRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<ObjectiveOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _objectiveRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<ObjectiveOutgoingDto>> GetAllAsync()
    {
        var entities = await _objectiveRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }
}
