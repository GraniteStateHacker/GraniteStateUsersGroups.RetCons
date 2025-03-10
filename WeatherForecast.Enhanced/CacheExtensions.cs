
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WeatherForecast.Enhanced;

public static class CacheExtensions
{
    public static T Cache<T>(this IDistributedCache cache, string key, Func<T> factory)
    {
        T value;
        var bytes = cache.Get(key);
        if (bytes != null)
        {
            value = JsonSerializer.Deserialize<T>(bytes)!;
        }
        else
        {
            value = factory();
            bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            cache.Set(key, bytes, new DistributedCacheEntryOptions());
        }
        return value;
    }
}
