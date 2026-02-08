// Web/Configuration/RedisConfigurationExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using PracticalWork.Library.Cache.Redis;
using StackExchange.Redis;

namespace PracticalWork.Library.Web.Configuration
{
    public static class RedisConfigurationExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var redisSection = configuration.GetSection("Redis");
            var connectionString = redisSection["ConnectionString"];
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured. Please add 'Redis:ConnectionString' to appsettings.json");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
                options.InstanceName = redisSection["InstanceName"] ?? "LibraryCache";
            });

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(connectionString, true);
                config.AbortOnConnectFail = false;
                config.ConnectTimeout = 30000;
                config.SyncTimeout = 30000;
                config.ReconnectRetryPolicy = new LinearRetry(5000);
                
                var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
                logger.LogInformation("Connecting to Redis at {EndPoints}", 
                    string.Join(", ", config.EndPoints));
                
                return ConnectionMultiplexer.Connect(config);
            });

            return services;
        }
    }
}