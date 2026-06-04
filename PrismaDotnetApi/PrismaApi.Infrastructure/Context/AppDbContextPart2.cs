using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;

namespace PrismaApi.Infrastructure.Context;

public partial class AppDbContext : DbContext
{
    private void InvalidateAssessmentsCache()
    {

        HashSet<Guid> affectedProjectIds =
        [
            ..GetChangedEntries<Assessment>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<DecisionQualityAssessment>().Select(e => e.Entity.ProjectId),
        ];

        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetAssessmentKey(projectId) });
        }
    }

    private void InvalidateBoardNodesCache()
    {
        var affectedProjectIds = GetChangedEntries<BoardNode>()
            .Select(e => e.Entity.ProjectId)
            .ToHashSet();
        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetBoardNodesInProjectKey(projectId) });
        }
    }


    private async Task InvalidateProjectUsersAsync()
    {
        var projectRolesEntries = GetChangedEntries<ProjectRole>().ToList();

        HashSet<string> affectedUserIds =
            [.. GetChangedEntries<User>().Select(e => e.Entity.Id)];

        foreach (var entry in projectRolesEntries)
        {
            if (AppOptions.IsPublicInstance)
            {
                var userName = entry.Entity.User?.Name
                    ?? await Users.Where(u => u.Id == entry.Entity.UserId).Select(u => u.Name).FirstAsync();
                affectedUserIds.Add(userName);
            }
            else
            {
                affectedUserIds.Add(entry.Entity.UserId);
            }
        }

        foreach (var userId in affectedUserIds)
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetUserKey(userId) });
    }

    private void InvalidateInfluenceDiagramData()
    {
        HashSet<Guid> affectedProjectIds =
        [
            ..GetChangedEntries<ProjectRole>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Edge>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Issue>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Node>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Uncertainty>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Decision>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Option>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<Outcome>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<DiscreteProbability>().Select(e => e.Entity.ProjectId),
            ..GetChangedEntries<DiscreteUtility>().Select(e => e.Entity.ProjectId),
        ];

        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetInfluenceDiagramKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetIssuesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetEdgesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetNodesInProjectKey(projectId) });
        }
    }

    private void InvalidatePublicProjectsCache()
    {
        var changedProjects = GetChangedEntries<Project>().ToList();
        if (changedProjects.Count == 0)
            return;

        var publicIds = _cache.GetPublicProjectIds();
        var changed = false;

        foreach (var entry in changedProjects)
        {
            var projectId = entry.Entity.Id;

            if (entry.State == EntityState.Deleted || !entry.Entity.Public)
            {
                if (publicIds.Remove(projectId))
                    changed = true;
            }
            else if (entry.Entity.Public)
            {
                if (publicIds.Add(projectId))
                    changed = true;
            }
        }

        if (changed)
        {
            _cache.Set(CacheKeys.PublicProjectIdsKey, publicIds);
        }
    }

    private async Task InvalidateCacheAsync()
    {
        InvalidatePublicProjectsCache();
        InvalidateAssessmentsCache();
        InvalidateBoardNodesCache();
        InvalidateInfluenceDiagramData();
        await InvalidateProjectUsersAsync();
    }
}
