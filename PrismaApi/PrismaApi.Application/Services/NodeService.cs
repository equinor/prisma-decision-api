using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class NodeService: INodeService
{
    private readonly INodeRepository _nodeRepository;

    public NodeService(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<List<NodeOutgoingDto>> UpdateAsync(List<NodeIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _nodeRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _nodeRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _nodeRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<NodeOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var entities = await _nodeRepository.GetByIdsAsync(ids, withTracking: false);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<NodeOutgoingDto>> GetAllAsync()
    {
        var entities = await _nodeRepository.GetAllAsync(withTracking: false);
        return entities.ToOutgoingDtos();
    }
}
