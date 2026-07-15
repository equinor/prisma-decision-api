using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;
using System.Linq.Expressions;

namespace PrismaApi.Application.Services;

public class EdgeService : IEdgeService
{
    private readonly IEdgeRepository _edgeRepository;
    private readonly IMemoryCache _cache;

    public EdgeService(IEdgeRepository edgeRepository, IMemoryCache cache)
    {
        _edgeRepository = edgeRepository;
        _cache = cache;
    }

    public async Task<List<EdgeOutgoingDto>> CreateAsync(List<EdgeIncomingDto> dtos, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities();
        await _edgeRepository.AddRangeAsync(entities, ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var created = await _edgeRepository.GetByIdsAsync(ids, withTracking: false, ct: ct);
        return created.ToOutgoingDtos();
    }

    public async Task<List<EdgeOutgoingDto>> UpdateAsync(List<EdgeIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities();
        await _edgeRepository.UpdateRangeAsync(entities, UserFilter(userDto), ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _edgeRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        await _edgeRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    public async Task<List<EdgeOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var entities = await _edgeRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<EdgeOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var edges = new List<EdgeOutgoingDto>();
        var projectIdsToGetFromDb = new HashSet<Guid>();

        var projectIds = _cache.GetAccessibleProjectIds(user);

        foreach (var projectId in projectIds)
        {
            var cachedEdges = _cache.GetCacheItemAsEdges(projectId, user);
            if (cachedEdges != null)
            {
                edges.AddRange(cachedEdges);
            }
            else
            {
                projectIdsToGetFromDb.Add(projectId);
            }
        }

        if (projectIdsToGetFromDb.Count > 0)
        {
            var projectEdges = await _edgeRepository.GetAllAsync(withTracking: false, filterPredicate: ProjectFilter(projectIdsToGetFromDb), ct: ct);
            var edgeDtos = projectEdges.ToOutgoingDtos();
            edges.AddRange(edgeDtos);
            foreach (var projectId in projectIdsToGetFromDb)
            {
                var cacheKey = CacheKeys.GetEdgesInProjectKey(projectId);
                var projectEdgeDtos = edgeDtos.Where(e => e.ProjectId == projectId).ToList();
                _cache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, CacheConstants.DefaultQueryCacheInTimeSpan, projectEdgeDtos);
            }
        }
        return edges;
    }
    private static Expression<Func<Edge, bool>> UserFilter(UserOutgoingDto user)
        => e => e.HeadNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id) && e.TailNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
    private static Expression<Func<Edge, bool>> ProjectFilter(HashSet<Guid> projectIds)
        => e => projectIds.Contains(e.ProjectId);
}
