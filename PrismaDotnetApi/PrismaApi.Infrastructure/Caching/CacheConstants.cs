namespace PrismaApi.Infrastructure.Caching;

public static class CacheConstants
{
    /// <summary>
    ///     Default duration suggestion for local memory cache.
    /// </summary>
    public static int DefaultQueryCacheInMinutes => 3;
    public static TimeSpan DefaultQueryCacheInTimeSpan => TimeSpan.FromMinutes(DefaultQueryCacheInMinutes);

    public static int DefaultMediumQueryCacheInMinutes => 10;
    public static TimeSpan DefaultMediumQueryCacheInTimeSpan => TimeSpan.FromMinutes(DefaultMediumQueryCacheInMinutes);
    public static int DefaultLongQueryCacheInMinutes => 30;
    public static TimeSpan DefaultLongQueryCacheInTimeSpan => TimeSpan.FromMinutes(DefaultLongQueryCacheInMinutes);

    /// <summary>
    ///     If no default value is used (eg. by using DefaultQueryCacheInMinutes, the following sliding
    ///     duration will be used.
    /// </summary>
    public static int DefaultMemoryCacheSlidingDurationInMinutes => DefaultQueryCacheInMinutes;

    /// <summary>
    ///     Default sliding duration for redis cache.
    /// </summary>
    public static int DefaultRedisCacheSlidingDurationInMinutes =>
        DefaultMemoryCacheSlidingDurationInMinutes + 1;
}
