using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;

namespace PracticalWork.Library.Cache.Redis
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;
        public IConnectionMultiplexer Connection { get; }

        public CacheService(IDistributedCache cache, IConnectionMultiplexer connection)
        {
            _cache = cache;
            Connection = connection;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var json = await _cache.GetStringAsync(key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }

        public Task RemoveAsync(string key)
        {
            return _cache.RemoveAsync(key);
        }
    }
}