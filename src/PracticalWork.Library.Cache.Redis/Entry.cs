using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace PracticalWork.Library.Cache.Redis
{
    public static class Entry
    {
        /// <summary>
        /// Регистрация зависимостей для распределенного Redis Cache
        /// </summary>
        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["App:Redis:RedisCacheConnection"];
            var prefix = configuration["App:Redis:RedisCachePrefix"] ?? "app_cache:";

            // Регистрируем подключение к Redis (Singleton)
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(connectionString);
            });

            // Настраиваем IDistributedCache через Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
                options.InstanceName = prefix; // Префикс для всех ключей
            });

            return services;
        }
    }
}