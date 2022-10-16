namespace API.Cache;

using System.Collections.Concurrent;
using API.Cache.Contracts;

public class InMemoryCacheStore : ICacheStore
{
    private long _latestRevisionNumber;
    private readonly ConcurrentDictionary<CacheKey, CacheRecord> _cachedRecords = new ();

    public Task<CacheRecord> GetAsync(CacheKey key, CancellationToken cancellationToken) => Task.FromResult(this.GetInternally(key));
    public Task<long> GetRevisionNumberAsync(CacheKey key, CancellationToken cancellationToken) => Task.FromResult(this._latestRevisionNumber);

    public Task<bool> SetAsync(long revisionNumber, CacheKey key, CacheRecord record, CancellationToken cancellationToken) => Task.FromResult(this.SetInternally(revisionNumber, key, record));

    public Task<long> InvalidateAsync(CacheKey cacheKey, CancellationToken cancellationToken) => Task.FromResult(this.InvalidateInternally(cacheKey));

    private CacheRecord GetInternally(CacheKey key)
    {
        if (key is not null && this._cachedRecords.TryGetValue(key, out var record)) return record;
        return null;
    }

    private bool SetInternally(long revisionNumber, CacheKey key, CacheRecord record)
    {
        if (key is null || record is null || this._latestRevisionNumber != revisionNumber) return false;

        this._cachedRecords[key] = record;
        return true;
    }

    private long InvalidateInternally(CacheKey cacheKey)
    {
        // NOTE: This is not an example of smart or optimal invalidation. We will invalidate all requests that have the same path root, e.g. if we modify a product, we will invalidate all shop requests.
        // It is possible to improve this behavior, however, since our time is limited, we can use this infrastructure as a figurative example of what should happen.
        this._cachedRecords.Clear();
        return Interlocked.Increment(ref this._latestRevisionNumber);
    }
}