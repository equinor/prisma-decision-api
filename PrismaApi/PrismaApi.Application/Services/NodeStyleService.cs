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

public class NodeStyleService: INodeStyleService
{
    private readonly INodeStyleRepository _nodeStyleRepository;

    public NodeStyleService(INodeStyleRepository nodeStyleRepository)
    {
        _nodeStyleRepository = nodeStyleRepository;
    }

    public async Task<List<NodeStyleOutgoingDto>> UpdateAsync(List<NodeStyleIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities();
        await _nodeStyleRepository.UpdateRangeAsync(entities, UserFilter(userDto));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _nodeStyleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _nodeStyleRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<NodeStyleOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var entities = await _nodeStyleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    public async Task<List<NodeStyleOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var entities = await _nodeStyleRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return entities.ToOutgoingDtos();
    }

    private static Expression<Func<NodeStyle, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Node!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
