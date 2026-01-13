// Infrastructure/Services/HybridCacheService.cs
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services
{
    public class HybridCacheService : ICacheService
    {
        private readonly IDistributedCache? _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<HybridCacheService> _logger;
        private bool _useDistributedCache;
        private DateTime _lastRedisCheckTime;
        private readonly TimeSpan _redisRetryInterval = TimeSpan.FromMinutes(5);

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public HybridCacheService(
            IDistributedCache? distributedCache,
            IMemoryCache memoryCache,
            ILogger<HybridCacheService> logger)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _logger = logger;
            _lastRedisCheckTime = DateTime.MinValue;

            // ✅ Initial Redis availability check
            _useDistributedCache = CheckRedisAvailability();

            _logger.LogInformation(
                "🔧 Cache Service Initialized: {CacheType}",
                _useDistributedCache ? "✅ Redis (Distributed Cache)" : "⚠️ In-Memory Cache (Fallback)");
        }

        /// <summary>
        /// Check if Redis is available and working
        /// </summary>
        private bool CheckRedisAvailability()
        {
            if (_distributedCache == null)
            {
                _logger.LogWarning("⚠️ IDistributedCache not injected - using in-memory cache");
                return false;
            }

            try
            {
                // Test Redis connection with a quick ping
                var testKey = $"__redis_health_check_{Guid.NewGuid()}";
                var testValue = "ping";

                _distributedCache.SetString(testKey, testValue, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                });

                var retrievedValue = _distributedCache.GetString(testKey);
                _distributedCache.Remove(testKey);

                if (retrievedValue == testValue)
                {
                    _logger.LogInformation("✅ Redis connection verified successfully");
                    _lastRedisCheckTime = DateTime.UtcNow;
                    return true;
                }

                _logger.LogWarning("⚠️ Redis health check failed - value mismatch");
                return false;
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis connection failed (invalid URL or server down): {Message}. Using in-memory cache.",
                    ex.Message);
                return false;
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis connection timeout (server unreachable): {Message}. Using in-memory cache.",
                    ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis availability check failed: {Message}. Using in-memory cache.",
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Periodically retry Redis connection if it was previously unavailable
        /// </summary>
        private void TryReconnectRedis()
        {
            if (!_useDistributedCache &&
                DateTime.UtcNow - _lastRedisCheckTime > _redisRetryInterval)
            {
                _logger.LogInformation("🔄 Attempting to reconnect to Redis...");
                _useDistributedCache = CheckRedisAvailability();

                if (_useDistributedCache)
                {
                    _logger.LogInformation("✅ Successfully reconnected to Redis");
                }
            }
        }

        public async Task<T> GetOrCreateAsync<T>(
            string cacheKey,
            Func<Task<T>> factory,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ✅ Periodically try to reconnect to Redis
                TryReconnectRedis();

                // Try get from cache
                var cachedValue = await GetAsync<T>(cacheKey, cancellationToken);
                if (cachedValue != null)
                {
                    _logger.LogDebug("🎯 Cache HIT: {CacheKey} ({CacheType})",
                        cacheKey,
                        _useDistributedCache ? "Redis" : "Memory");
                    return cachedValue;
                }

                _logger.LogDebug("❌ Cache MISS: {CacheKey}", cacheKey);

                // Execute factory to get fresh data
                var value = await factory();

                // Store in cache
                await SetAsync(cacheKey, value, absoluteExpiration, slidingExpiration, cancellationToken);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Cache error for key: {CacheKey}. Falling back to factory.", cacheKey);
                return await factory();
            }
        }

        public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_useDistributedCache)
            {
                return await GetFromDistributedCacheAsync<T>(cacheKey, cancellationToken);
            }

            return GetFromMemoryCache<T>(cacheKey);
        }

        public async Task SetAsync<T>(
            string cacheKey,
            T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default)
        {
            if (_useDistributedCache)
            {
                await SetToDistributedCacheAsync(cacheKey, value, absoluteExpiration, slidingExpiration, cancellationToken);
            }
            else
            {
                SetToMemoryCache(cacheKey, value, absoluteExpiration, slidingExpiration);
            }
        }

        public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_useDistributedCache)
            {
                await RemoveFromDistributedCacheAsync(cacheKey, cancellationToken);
            }
            else
            {
                RemoveFromMemoryCache(cacheKey);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            if (!_useDistributedCache)
            {
                RemoveMemoryCacheByPattern(pattern);
            }
            else
            {
                _logger.LogWarning("Pattern removal not implemented for Redis: {Pattern}", pattern);
            }

            await Task.CompletedTask;
        }

        #region Distributed Cache Methods

        private async Task<T?> GetFromDistributedCacheAsync<T>(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                var cachedData = await _distributedCache!.GetStringAsync(cacheKey, cancellationToken);
                if (string.IsNullOrEmpty(cachedData))
                    return default;

                return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis GET failed (connection lost): {CacheKey}. Switching to in-memory cache.",
                    cacheKey);

                // ✅ Permanently switch to memory cache until retry
                _useDistributedCache = false;
                return GetFromMemoryCache<T>(cacheKey);
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis GET timeout: {CacheKey}. Switching to in-memory cache.",
                    cacheKey);

                _useDistributedCache = false;
                return GetFromMemoryCache<T>(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "⚠️ Failed to get from Redis: {CacheKey}. Trying memory cache.",
                    cacheKey);

                return GetFromMemoryCache<T>(cacheKey);
            }
        }

        private async Task SetToDistributedCacheAsync<T>(
            string cacheKey,
            T value,
            TimeSpan? absoluteExpiration,
            TimeSpan? slidingExpiration,
            CancellationToken cancellationToken)
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(value, _jsonOptions);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(10),
                    SlidingExpiration = slidingExpiration
                };

                await _distributedCache!.SetStringAsync(cacheKey, serializedData, options, cancellationToken);

                _logger.LogDebug("✅ Cache SET (Redis): {CacheKey}, Expiration: {Expiration}",
                    cacheKey, absoluteExpiration ?? TimeSpan.FromMinutes(10));
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis SET failed (connection lost): {CacheKey}. Switching to in-memory cache.",
                    cacheKey);

                // ✅ Switch to memory cache
                _useDistributedCache = false;
                SetToMemoryCache(cacheKey, value, absoluteExpiration, slidingExpiration);
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex,
                    "❌ Redis SET timeout: {CacheKey}. Switching to in-memory cache.",
                    cacheKey);

                _useDistributedCache = false;
                SetToMemoryCache(cacheKey, value, absoluteExpiration, slidingExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "⚠️ Failed to set Redis cache: {CacheKey}. Falling back to memory cache.",
                    cacheKey);

                SetToMemoryCache(cacheKey, value, absoluteExpiration, slidingExpiration);
            }
        }

        private async Task RemoveFromDistributedCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                await _distributedCache!.RemoveAsync(cacheKey, cancellationToken);
                _logger.LogDebug("🗑️ Cache REMOVED (Redis): {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove from Redis: {CacheKey}", cacheKey);

                // ✅ Also try removing from memory cache as fallback
                RemoveFromMemoryCache(cacheKey);
            }
        }

        #endregion

        #region Memory Cache Methods

        private T? GetFromMemoryCache<T>(string cacheKey)
        {
            try
            {
                if (_memoryCache.TryGetValue(cacheKey, out T? cachedValue))
                {
                    return cachedValue;
                }
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get from memory cache: {CacheKey}", cacheKey);
                return default;
            }
        }

        private void SetToMemoryCache<T>(
            string cacheKey,
            T value,
            TimeSpan? absoluteExpiration,
            TimeSpan? slidingExpiration)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(10),
                    SlidingExpiration = slidingExpiration,
                    Size = 1
                };

                _memoryCache.Set(cacheKey, value, cacheOptions);

                _logger.LogDebug("✅ Cache SET (Memory): {CacheKey}, Expiration: {Expiration}",
                    cacheKey, absoluteExpiration ?? TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set memory cache: {CacheKey}", cacheKey);
            }
        }

        private void RemoveFromMemoryCache(string cacheKey)
        {
            try
            {
                _memoryCache.Remove(cacheKey);
                _logger.LogDebug("🗑️ Cache REMOVED (Memory): {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove from memory cache: {CacheKey}", cacheKey);
            }
        }

        private void RemoveMemoryCacheByPattern(string pattern)
        {
            _logger.LogWarning("Pattern removal for memory cache not fully implemented: {Pattern}", pattern);
        }

        #endregion
    }
}
