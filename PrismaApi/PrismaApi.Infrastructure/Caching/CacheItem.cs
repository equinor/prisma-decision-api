namespace PrismaApi.Infrastructure.Caching;

public class CacheItem : IEquatable<CacheItem>
{
    public string CacheKey { get; init; } = default!;
    public bool IsGlobal { get; init; }


    public bool Equals(CacheItem? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return CacheKey == other.CacheKey && IsGlobal == other.IsGlobal;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CacheItem)obj);
    }

    public override int GetHashCode() => HashCode.Combine(CacheKey, IsGlobal);
}

