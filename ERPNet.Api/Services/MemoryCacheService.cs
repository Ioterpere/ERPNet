using ERPNet.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ERPNet.Api.Services;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public T? Get<T>(string key)
    {
        cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };

        cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        cache.Remove(key);
    }
}
