namespace API.Cache;

using System.Collections.Concurrent;
using API.Cache.Contracts;

public class InMemoryCacheStore : ICacheStore
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<CacheKey>> _keysByPath = new ();
    private readonly ConcurrentDictionary<CacheKey, CacheRecord> _cachedRecords = new ();

    public Task<CacheRecord> GetAsync(CacheKey key, CancellationToken cancellationToken) => Task.FromResult(this.GetInternally(key));

    public Task SetAsync(CacheKey key, CacheRecord record, CancellationToken cancellationToken)
    {
        this.SetInternally(key, record);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string requestPath, CancellationToken cancellationToken)
    {
        this.InvalidateInternally(requestPath);
        return Task.CompletedTask;
    }

    private CacheRecord GetInternally(CacheKey key)
    {
        if (key is not null && this._cachedRecords.TryGetValue(key, out var record)) return record;
        return null;
    }

    private void SetInternally(CacheKey key, CacheRecord record)
    {
        if (key is null || record is null) return;

        if (string.IsNullOrWhiteSpace(key.RequestPath) == false)
        {
            var commonKeys = this._keysByPath.GetOrAdd(key.RequestPath, _ => new ConcurrentBag<CacheKey>());
            commonKeys.Add(key);
        }

        this._cachedRecords[key] = record;
    }

    private void InvalidateInternally(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath) || this._keysByPath.TryGetValue(requestPath, out var affectedKeys) == false) return;
        foreach (var key in affectedKeys) this._cachedRecords.TryRemove(key, out _);
    }
}