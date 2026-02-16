using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class NodeService
{
    private readonly NodeRepository _nodeRepository;

    public NodeService(NodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<List<NodeOutgoingDto>> UpdateAsync(List<NodeIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _nodeRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _nodeRepository.GetByIdsAsync(ids);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _nodeRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<NodeOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _nodeRepository.GetByIdsAsync(ids);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<NodeOutgoingDto>> GetAllAsync()
    {
        var entities = await _nodeRepository.GetAllAsync();
        return entities.ToOutgoingDtos();
    }
}
