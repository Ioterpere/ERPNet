using System.Collections.Concurrent;
using ERPNet.Application.Auth;
using ERPNet.Application.Cache;
using ERPNet.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ERPNet.Api.Services;

public class MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> cacheSettings) : ICacheService
{
    private readonly CacheSettings _cacheSettings = cacheSettings.Value;
    private readonly ConcurrentDictionary<string, bool> _keys = new();

    public T? Get<T>(string key)
    {
        cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes)
        };

        options.RegisterPostEvictionCallback((k, _, _, _) => _keys.TryRemove(k.ToString()!, out _));

        _keys.TryAdd(key, true);
        cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _keys.TryRemove(key, out _);
        cache.Remove(key);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)))
            Remove(key);
    }
}
