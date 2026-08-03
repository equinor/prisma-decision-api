using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PrismaApi.Application.Services;

public class DiscreteProbabilityService: IDiscreteProbabilityService
{
    private readonly IDiscreteProbabilityRepository _discreteProbabilityRepository;
    private readonly IMemoryCache _cache;

    public DiscreteProbabilityService(IDiscreteProbabilityRepository discreteProbabilityRepository, IMemoryCache cache)
    {
        _discreteProbabilityRepository = discreteProbabilityRepository;
        _cache = cache;
    }

    public async Task<List<DiscreteProbabilityDto>> CreateAsync(List<DiscreteProbabilityDto> dtos, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities();
        await _discreteProbabilityRepository.AddRangeAsync(entities, ct);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteProbabilityDto>> UpdateAsync(List<DiscreteProbabilityDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntitiesWithoutParents();
        await _discreteProbabilityRepository.UpdateRangeAsync(entities, UserFilter(userDto), ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _discreteProbabilityRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
        return updated.ToDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        await _discreteProbabilityRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    public async Task<List<DiscreteProbabilityDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var entities = await _discreteProbabilityRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return entities.ToDtos();
    }

    public async Task<List<DiscreteProbabilityDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var discreteProbabilities = new List<DiscreteProbabilityDto>();
        var projectIdsToGetFromDb = new HashSet<Guid>();

        var projectIds = _cache.GetAccessibleProjectIds(user);

        foreach (var projectId in projectIds)
        {
            var cachedDiscreteProbabilities = _cache.GetCacheItemAsDiscreteProbabilities(projectId, user);
            if (cachedDiscreteProbabilities != null)
            {
                discreteProbabilities.AddRange(cachedDiscreteProbabilities);
            }
            else
            {
                projectIdsToGetFromDb.Add(projectId);
            }
        }

        if (projectIdsToGetFromDb.Count > 0)
        {
            var projectDiscreteProbabilities = await _discreteProbabilityRepository.GetAllAsync(withTracking: false, filterPredicate: ProjectFilter(projectIdsToGetFromDb), ct: ct);
            var discreteProbabilityDtos = projectDiscreteProbabilities.ToDtos();
            discreteProbabilities.AddRange(discreteProbabilityDtos);
            foreach (var projectId in projectIdsToGetFromDb)
            {
                var cacheKey = CacheKeys.GetDiscreteProbabilitiesInProjectKey(projectId);
                var projectDiscreteProbabilityDtos = discreteProbabilityDtos.Where(dp => dp.ProjectId == projectId).ToList();
                _cache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, CacheConstants.DefaultQueryCacheInTimeSpan, projectDiscreteProbabilityDtos);
            }
        }
        return discreteProbabilities;
    }

    private static Expression<Func<DiscreteProbability, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Uncertainty!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<DiscreteProbability, bool>> ProjectFilter(HashSet<Guid> projectIds)
        => e => projectIds.Contains(e.ProjectId);
}
