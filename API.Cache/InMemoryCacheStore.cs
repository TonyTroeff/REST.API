namespace API.Cache;

using System.Collections.Concurrent;
using API.Cache.Contracts;

public class InMemoryCacheStore : ICacheStore
{
    private readonly ConcurrentDictionary<CacheKey, CacheRecord> _cachedRecords = new ();

    public Task<CacheRecord> GetAsync(CacheKey key, CancellationToken cancellationToken) => Task.FromResult(this.GetInternally(key));

    public Task SetAsync(CacheKey key, CacheRecord record, CancellationToken cancellationToken)
    {
        this.SetInternally(key, record);
        return Task.CompletedTask;
    }

    public Task<bool> InvalidateAsync(string requestPath, CancellationToken cancellationToken) => Task.FromResult(this.InvalidateInternally(requestPath));

    private CacheRecord GetInternally(CacheKey key)
    {
        if (key is not null && this._cachedRecords.TryGetValue(key, out var record)) return record;
        return null;
    }

    private void SetInternally(CacheKey key, CacheRecord record)
    {
        if (key is null || record is null) return;

        this._cachedRecords[key] = record;
    }

    private bool InvalidateInternally(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath)) return false;

        // NOTE: This is not the most optimal approach. For the in-memory scenario, you can use a tree to easily reduce the execution time of this logic.
        HashSet<CacheKey> keysToInvalidate = new ();
        foreach (var key in this._cachedRecords.Keys)
        {
            if (string.IsNullOrWhiteSpace(key.RequestPath) == false && key.RequestPath.StartsWith(key.RequestPath, StringComparison.OrdinalIgnoreCase))
                keysToInvalidate.Add(key);
        }

        var success = true;
        foreach (var key in keysToInvalidate)
        {
            if (this._cachedRecords.TryRemove(key, out _) == false) 
                success = false;
        }

        return success;
    }
}