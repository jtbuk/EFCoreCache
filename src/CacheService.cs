using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;
using System.Text.Json;

namespace Jtbuk.EFCoreCache;


public interface ICacheService
{
    Task Set(string key, object value, TimeSpan? cacheDuration = null);
    Task<T?> Get<T>(string key);
    Task Remove(string key);
    Task<T?> GetAndSet<T>(string key, Func<Task<T>> getFunction, TimeSpan? cacheDuration = null);
}

public class DistributedCacheService : ICacheService
{
    private const int DEFAULT_CACHE_DURATION_HOURS = 24;
    private readonly string VERSION = Assembly.GetExecutingAssembly().GetName()!.Version!.ToString();
    private readonly IDistributedCache _cache;

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;        
    }

    public async Task Set(string key, object value, TimeSpan? cacheDuration = null)
    {
        if (value != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration == null ? TimeSpan.FromHours(DEFAULT_CACHE_DURATION_HOURS) : cacheDuration,
            };

            var jsonString = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(GetFormattedCacheKey(key), jsonString, options);
        }
    }

    public async Task<T?> GetAndSet<T>(string key, Func<Task<T>> getFunction, TimeSpan? cacheDuration = null)
    {
        var item = await Get<T>(key);
        
        if (item != null) return item;

        item = await getFunction();

        if (item == null) return item;

        await Set(key, item, cacheDuration);

        return item;
    }

    public async Task<T?> Get<T>(string key)
    {
        var jsonString = await _cache.GetStringAsync(GetFormattedCacheKey(key));

        if (jsonString == null) return default;

        return JsonSerializer.Deserialize<T>(jsonString);
    }

    public async Task Remove(string key)
    {
        await _cache.RemoveAsync(GetFormattedCacheKey(key));
    }

    public string GetFormattedCacheKey(string key)
    {
        //Prevent stale data after an update
        //This is also where you might want to implement a tenant key i.e. VERSION:TENANT:key

        return $"{VERSION}:{key}";
    }
}
