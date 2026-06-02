using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Infrastructure.Caching;

public static class MemoryCacheExtensions
{
    private static readonly HashSet<CacheItem> cachedKeys = new();
    private static readonly SemaphoreSlim cacheLock = new(1, 1);

    private static readonly MemoryCacheEntryOptions CacheEntryOptions =
        new MemoryCacheEntryOptions().SetSlidingExpiration(
            TimeSpan.FromMinutes(CacheConstants.DefaultMemoryCacheSlidingDurationInMinutes));

    public static object? GetCacheItem(this IMemoryCache cache, string cacheKey)
    {
        if (cache.TryGetValue(cacheKey, out var value))
        {
            return value;
        }

        return null;
    }

    public static T? GetCacheItem<T>(this IMemoryCache cache, string cacheKey) where T : class
    {
        if (cache.TryGetValue(cacheKey, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return null;
    }

    public static InfluenceDiagramDto? GetCacheItemAsInfluenceDiagram(this IMemoryCache cache, Guid projectId, UserOutgoingDto user)
    {
        // check that the user has access to the project before returning cached diagram
        if (!user.HasAccessToProject(projectId))
        {
            return null;
        }
        return cache.GetCacheItem<InfluenceDiagramDto>(CacheKeys.GetInfluenceDiagramKey(projectId));  
    }

    public static List<IssueOutgoingDto>? GetCacheItemAsIssues(this IMemoryCache cache, Guid projectId, UserOutgoingDto user)
    {
        // check that the user has access to the project before returning cached issues
        if (!user.HasAccessToProject(projectId))
        {
            return null;
        }
        return cache.GetCacheItem<List<IssueOutgoingDto>>(CacheKeys.GetIssuesInProjectKey(projectId));
    }

     public static List<EdgeOutgoingDto>? GetCacheItemAsEdges(this IMemoryCache cache, Guid projectId, UserOutgoingDto user)
    {
        // check that the user has access to the project before returning cached edges
        if (!user.HasAccessToProject(projectId))
        {
            return null;
        }
        return cache.GetCacheItem<List<EdgeOutgoingDto>>(CacheKeys.GetEdgesInProjectKey(projectId));
    }

    public static List<NodeOutgoingDto>? GetCacheItemAsNodes(this IMemoryCache cache, Guid projectId, UserOutgoingDto user)
    {
        // check that the user has access to the project before returning cached nodes
        if (!user.HasAccessToProject(projectId))
        {
            return null;
        }
        return cache.GetCacheItem<List<NodeOutgoingDto>>(CacheKeys.GetNodesInProjectKey(projectId));
    }

    public static List<BoardNodeOutgoingDto>? GetCacheItemAsBoardNodes(this IMemoryCache cache, Guid projectId, UserOutgoingDto user)
    {
        // check that the user has access to the project before returning cached board nodes
        if (!user.HasAccessToProject(projectId))
        {
            return null;
        }
        return cache.GetCacheItem<List<BoardNodeOutgoingDto>>(CacheKeys.GetBoardNodesInProjectKey(projectId));
    }

    public static List<AssessmentOutgoingDto>? GetCacheItemAsAssessment(this IMemoryCache cache, Guid projectId, UserOutgoingDto user)
    {
        // check that the user has access to the project before returning cached assessment
        if (!user.HasAccessToProject(projectId))
        {
            return null;
        }
        return cache.GetCacheItem<List<AssessmentOutgoingDto>>(CacheKeys.GetAssessmentKey(projectId));
    }

    public static void AddCacheItem(this IMemoryCache cache, CacheItem key, TimeSpan? duration,
        object? value)
    {
        // We do not want to cache null values
        if (value is null)
        {
            return;
        }

        cacheLock.Wait();
        try
        {
            if (duration.HasValue)
            {
                _ = cache.Set(key.CacheKey, value, duration.Value);
            }
            else
            {
                // Create cache entry with sliding expiration for all entries not having a duration provided. This to prevent cache to always grow.
                _ = cache.Set(key.CacheKey, value, CacheEntryOptions);
            }

            _ = cachedKeys.Add(key);
        }
        finally
        {
            _ = cacheLock.Release();
        }
    }

    public static void InvalidateCacheEntry(this IMemoryCache cache, CacheItem cacheItem)
    {
        cacheLock.Wait();
        try
        {
            if (cacheItem.IsGlobal)
            {
                cache.InvalidateGloballyCachedQueries();
            }
            else
            {
                cache.InvalidateCachedQueriesByKey(cacheItem);
            }
        }
        finally
        {
            _ = cacheLock.Release();
        }
    }

    public static void InvalidateAllCachedQueries(this IMemoryCache cache)
    {
        var keysToInvalidate = cachedKeys.ToList();

        InvalidateCacheKeys(cache, keysToInvalidate);
    }

    private static void InvalidateGloballyCachedQueries(this IMemoryCache cache)
    {
        var keysToInvalidate = cachedKeys
            .Where(key => key.IsGlobal)
            .ToList();

        InvalidateCacheKeys(cache, keysToInvalidate);
    }

    private static void InvalidateCachedQueriesByKey(this IMemoryCache cache, CacheItem cacheKey)
    {
        var keysToInvalidate = cachedKeys
            .Where(key => key.IsGlobal == false && key.CacheKey == cacheKey.CacheKey);

        InvalidateCacheKeys(cache, keysToInvalidate);
    }

    private static void InvalidateCacheKeys(IMemoryCache cache,
        IEnumerable<CacheItem> keysToInvalidate)
    {
        foreach (var key in keysToInvalidate)
        {
            cache.Remove(key.CacheKey);
            _ = cachedKeys.Remove(key);
        }
    }

    public static double GetApproximateCacheSizeInMB(this IMemoryCache cache)
    {
        long totalBytes = 0;
        foreach (var key in cachedKeys)
        {
            var value = cache.GetCacheItem(key.CacheKey);
            if (value is not null)
            {
                var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
                totalBytes += json.Length;
            }
        }
        return Math.Round(totalBytes / (1024.0 * 1024.0), 4);
    }

    private static bool HasAccessToProject(this UserOutgoingDto user, Guid projectId)
        => user.ProjectRoles.Any(pr => pr.ProjectId == projectId);
}
