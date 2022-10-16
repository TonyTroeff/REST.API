namespace API.Cache.Contracts;

public interface ICacheStore
{
    Task<CacheRecord> GetAsync(CacheKey key, CancellationToken cancellationToken);
    Task<long> GetRevisionNumberAsync(CacheKey key, CancellationToken cancellationToken);
    Task<bool> SetAsync(long revisionNumber, CacheKey key, CacheRecord record, CancellationToken cancellationToken);
    Task<long> InvalidateAsync(CacheKey cacheKey, CancellationToken cancellationToken);
}