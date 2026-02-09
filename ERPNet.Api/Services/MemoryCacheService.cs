using ERPNet.Application.Auth;
using ERPNet.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ERPNet.Api.Services;

public class MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> cacheSettings) : ICacheService
{
    private readonly CacheSettings _cacheSettings = cacheSettings.Value;

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

        cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        cache.Remove(key);
    }
}
