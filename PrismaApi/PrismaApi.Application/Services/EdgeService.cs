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

public class EdgeService: IEdgeService
{
    private readonly IEdgeRepository _edgeRepository;

    public EdgeService(IEdgeRepository edgeRepository)
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
    private static Expression<Func<Edge, bool>> UserFilter(UserOutgoingDto user)
        => e => e.HeadNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id) && e.TailNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
