using StackExchange.Redis;

namespace PracticalWork.Library.Cache.Redis
{
    public static class CacheExtensions
    {
        public static async Task RemoveByPrefixAsync(this CacheService cache, string prefix)
        {
            var server = cache.Connection.GetServer(cache.Connection.GetEndPoints().First());
            foreach (var key in server.Keys(pattern: $"{prefix}*"))
            {
                await cache.RemoveAsync(key);
            }
        }
    }
}