using Microsoft.EntityFrameworkCore;

namespace Jtbuk.EFCoreCache
{
    public static class CacheExtensions
    {
        public static async Task<T?> TryCacheSingleOrDefaultAsync<T>(this IQueryable<T> query, ICacheService cacheService, string cacheKey, TimeSpan? cacheDuration = null)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await cacheService.GetAndSet<T>(cacheKey, async () => await query.SingleOrDefaultAsync(), cacheDuration);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
