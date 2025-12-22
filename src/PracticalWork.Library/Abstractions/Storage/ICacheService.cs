// Application/Interfaces/ICacheService.cs

using JetBrains.Annotations;

namespace PracticalWork.Library.Application.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с кэшем
    /// </summary>
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);
        Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }
}