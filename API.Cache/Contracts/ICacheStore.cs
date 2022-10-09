namespace API.Cache.Contracts;

public interface ICacheStore
{
    Task<CacheRecord> GetAsync(CacheKey key, CancellationToken cancellationToken);
    Task SetAsync(CacheKey key, CacheRecord record, CancellationToken cancellationToken);
    Task InvalidateAsync(string requestPath, CancellationToken cancellationToken);
}