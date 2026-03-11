using Microsoft.Extensions.Caching.Memory;

namespace PrismaApi.Infrastructure.Caching;

public static class MemoryCacheExtensions
{
    private static readonly HashSet<CacheItem> cachedKeys = new();
    private static readonly SemaphoreSlim cacheLock = new(1, 1);

    private static readonly MemoryCacheEntryOptions CacheEntryOptions =
        new MemoryCacheEntryOptions().SetSlidingExpiration(
            TimeSpan.FromMinutes(CacheConstants.DefaultMemoryCacheSlidingDurationInMinutes));

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
}
