using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetAssessmentKey(projectId) });
        }
    }

    private void InvalidateBoardNodesCache()
    {
        var affectedProjectIds = GetChangedEntries<BoardNode>()
            .Select(e => e.Entity.ProjectId)
            .ToHashSet();
        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetBoardNodesInProjectKey(projectId) });
        }
    }

    private async Task InvalidateInfluenceDiagramDataAsync()
    {
        var projectRolesEntries = GetChangedEntries<ProjectRole>().ToList();

        HashSet<Guid> affectedProjectIds =
        [
            ..projectRolesEntries.Select(e => e.Entity.ProjectId),
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
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var userId in affectedUserIds)
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetUserKey(userId) });

        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetInfluenceDiagramKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetIssuesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetEdgesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem { CacheKey = CacheKeys.GetNodesInProjectKey(projectId) });
        }
    }

    private async Task InvalidateCacheAsync()
    {
        InvalidateAssessmentsCache();
        InvalidateBoardNodesCache();
        await InvalidateInfluenceDiagramDataAsync();

    }
}
