using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class EdgeService
{
    private readonly EdgeRepository _edgeRepository;

    public EdgeService(EdgeRepository edgeRepository)
    {
        _edgeRepository = edgeRepository;
    }

    public async Task<List<EdgeOutgoingDto>> CreateAsync(List<EdgeIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _edgeRepository.AddRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var created = await _edgeRepository.GetByIdsAsync(ids, withTracking: false);
        return created.ToOutgoingDtos();
    }

    public async Task<List<EdgeOutgoingDto>> UpdateAsync(List<EdgeIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _edgeRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _edgeRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _edgeRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<EdgeOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _edgeRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<EdgeOutgoingDto>> GetAllAsync()
    {
        var entities = await _edgeRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }
}
