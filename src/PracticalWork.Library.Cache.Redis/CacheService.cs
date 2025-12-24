using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PracticalWork.Library.Application.Interfaces;
using StackExchange.Redis;

namespace PracticalWork.Library.Cache.Redis
{
    /// <summary>
    /// Сервис для работы с Redis кэшем
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly string _instancePrefix;

        public RedisCacheService(
            IDistributedCache distributedCache,
            IConnectionMultiplexer redisConnection,
            IConfiguration configuration,
            ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache;
            _redisConnection = redisConnection;
            _logger = logger;
            
            var redisSection = configuration.GetSection("Redis");
            _instancePrefix = redisSection["InstanceName"] ?? "Library";
            
            _logger.LogInformation("Redis configured with instance prefix: {Prefix}", _instancePrefix);
        }

        /// <summary>
        /// Получить данные из кэша
        /// </summary>
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = BuildFullKey(key);
                var json = await _distributedCache.GetStringAsync(fullKey, cancellationToken);
                
                if (string.IsNullOrEmpty(json))
                {
                    _logger.LogDebug("Ключ {Key} не найден в кэше", fullKey);
                    return default;
                }
                
                var result = JsonSerializer.Deserialize<T>(json);
                _logger.LogDebug("Данные по ключу {Key} получены из кэша", fullKey);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных из кэша по ключу {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Сохранить данные в кэш
        /// </summary>
        public async Task SetAsync<T>(
            string key, 
            T value, 
            TimeSpan? expiration = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = BuildFullKey(key);
                var json = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
                };
                
                await _distributedCache.SetStringAsync(fullKey, json, options, cancellationToken);
                _logger.LogDebug("Данные сохранены в кэш по ключу {Key} с TTL {Ttl}", 
                    fullKey, options.AbsoluteExpirationRelativeToNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении данных в кэш по ключу {Key}", key);
            }
        }

        /// <summary>
        /// Удалить данные из кэша
        /// </summary>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = BuildFullKey(key);
                await _distributedCache.RemoveAsync(fullKey, cancellationToken);
                _logger.LogDebug("Ключ {Key} удален из кэша", fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ключа {Key} из кэша", key);
            }
        }

        /// <summary>
        /// Удалить все ключи по префиксу
        /// </summary>
        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPrefix = BuildFullKey($"{prefix}*");
                var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
                var db = _redisConnection.GetDatabase();
                
                var keys = new List<RedisKey>();
                await foreach (var key in server.KeysAsync(pattern: fullPrefix))
                {
                    keys.Add(key);
                }
                
                if (keys.Any())
                {
                    await db.KeyDeleteAsync(keys.ToArray());
                    _logger.LogDebug("Удалено {Count} ключей с префиксом {Prefix}", 
                        keys.Count, prefix);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ключей по префиксу {Prefix}", prefix);
            }
        }

        /// <summary>
        /// Получить или установить данные (pattern GetOrSet)
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }

            return value;
        }

        /// <summary>
        /// Проверить существование ключа
        /// </summary>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = BuildFullKey(key);
                var json = await _distributedCache.GetStringAsync(fullKey, cancellationToken);
                return !string.IsNullOrEmpty(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования ключа {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Получить время жизни ключа
        /// </summary>
        public async Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = BuildFullKey(key);
                var db = _redisConnection.GetDatabase();
                var ttl = await db.KeyTimeToLiveAsync(fullKey);
                return ttl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении TTL для ключа {Key}", key);
                return null;
            }
        }

        /// <summary>
        /// Инкрементировать числовое значение
        /// </summary>
        public async Task<long> IncrementAsync(
            string key, 
            long value = 1, 
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fullKey = BuildFullKey(key);
                var db = _redisConnection.GetDatabase();
                var result = await db.StringIncrementAsync(fullKey, value);
                
                if (expiration.HasValue)
                    await db.KeyExpireAsync(fullKey, expiration.Value);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при инкременте ключа {Key}", key);
                return 0;
            }
        }

        /// <summary>
        /// Построить полный ключ с префиксом инстанса
        /// </summary>
        private string BuildFullKey(string key)
        {
            return $"{_instancePrefix}:{key}";
        }
    }
}