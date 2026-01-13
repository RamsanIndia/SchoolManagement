using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Get cached value or execute factory function if not found
        /// </summary>
        Task<T> GetOrCreateAsync<T>(
            string cacheKey,
            Func<Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get cached value
        /// </summary>
        Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set cache value
        /// </summary>
        Task SetAsync<T>(
            string cacheKey,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove cached value
        /// </summary>
        Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove all keys matching pattern
        /// </summary>
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    }
}
