using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class NodeStyleService
{
    private readonly NodeStyleRepository _nodeStyleRepository;

    public NodeStyleService(NodeStyleRepository nodeStyleRepository)
    {
        _nodeStyleRepository = nodeStyleRepository;
    }

    public async Task<List<NodeStyleOutgoingDto>> UpdateAsync(List<NodeStyleIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _nodeStyleRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _nodeStyleRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _nodeStyleRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<NodeStyleOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _nodeStyleRepository.GetByIdsAsync(ids);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<NodeStyleOutgoingDto>> GetAllAsync()
    {
        var entities = await _nodeStyleRepository.GetAllAsync();
        return entities.ToOutgoingDtos();
    }
}
