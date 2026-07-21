using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Caching
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<RedisCacheService>.Instance;
        }

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _database = _redis.GetDatabase();
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RedisCacheService>.Instance;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis SetAsync called with empty key");
                throw new ArgumentException("Key must not be null or empty", nameof(key));
            }

            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var set = expiry.HasValue
                    ? await _database.StringSetAsync(key, serializedValue, expiry.Value)
                    : await _database.StringSetAsync(key, serializedValue);
                if (!set)
                {
                    _logger.LogWarning("Redis SET failed for key {Key}", key);
                }
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Serialization error storing key {Key} in Redis", key);
                throw;
            }
            catch (RedisException rex)
            {
                _logger.LogError(rex, "Redis error on SET for key {Key}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on Redis SET for key {Key}", key);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis GetAsync called with empty key");
                throw new ArgumentException("Key must not be null or empty", nameof(key));
            }

            try
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("Redis MISS for key {Key}", key);
                    return default;
                }

                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(value.ToString());
                    _logger.LogDebug("Redis HIT for key {Key}", key);
                    return deserialized;
                }
                catch (JsonException jex)
                {
                    _logger.LogWarning(jex, "Deserialization error for key {Key}. Returning default<T>.", key);
                    return default;
                }
            }
            catch (RedisException rex)
            {
                _logger.LogError(rex, "Redis error on GET for key {Key}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on Redis GET for key {Key}", key);
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis RemoveAsync called with empty key");
                throw new ArgumentException("Key must not be null or empty", nameof(key));
            }

            try
            {
                var removed = await _database.KeyDeleteAsync(key);
                if (!removed)
                {
                    _logger.LogDebug("Redis DEL missed for key {Key}", key);
                }
            }
            catch (RedisException rex)
            {
                _logger.LogError(rex, "Redis error on DEL for key {Key}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on Redis DEL for key {Key}", key);
                throw;
            }
        }

        public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
        {
            if (keys is null)
            {
                _logger.LogWarning("Redis GetManyAsync called with null keys collection");
                throw new ArgumentNullException(nameof(keys));
            }

            try
            {
                var normalized = keys.Where(k => !string.IsNullOrWhiteSpace(k)).ToArray();
                var redisKeys = normalized.Select(k => (RedisKey)k).ToArray();

                if (redisKeys.Length == 0)
                    return new Dictionary<string, T?>();

                var values = await _database.StringGetAsync(redisKeys);

                var result = new Dictionary<string, T?>();
                for (int i = 0; i < redisKeys.Length; i++)
                {
                    var rawValue = values[i];
                    var key = redisKeys[i].ToString();
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    if (!rawValue.IsNullOrEmpty)
                    {
                        try
                        {
                            var deserialized = JsonSerializer.Deserialize<T>(rawValue.ToString());
                            result[key] = deserialized;
                        }
                        catch (JsonException jex)
                        {
                            _logger.LogWarning(jex, "Deserialization error for key {Key} in GetManyAsync. Setting default.", key);
                            result[key] = default;
                        }
                    }
                    else
                    {
                        result[key] = default;
                    }
                }

                return result;
            }
            catch (RedisException rex)
            {
                _logger.LogError(rex, "Redis error on MGET for {Count} keys", keys.Count());
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on Redis MGET");
                throw;
            }
        }

        public async Task<long> IncrementAsync(string key, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis IncrementAsync called with empty key");
                throw new ArgumentException("Key must not be null or empty", nameof(key));
            }

            try
            {
                // StringIncrementAsync is Redis INCR — atomic by specification.
                var newValue = await _database.StringIncrementAsync(key);

                // Refresh the TTL on every increment so an active counter
                // never expires mid-stream. EXPIRE is idempotent.
                if (expiry.HasValue)
                {
                    await _database.KeyExpireAsync(key, expiry);
                }

                return newValue;
            }
            catch (RedisException rex)
            {
                _logger.LogError(rex, "Redis error on INCR for key {Key}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on Redis INCR for key {Key}", key);
                throw;
            }
        }
    }
}
