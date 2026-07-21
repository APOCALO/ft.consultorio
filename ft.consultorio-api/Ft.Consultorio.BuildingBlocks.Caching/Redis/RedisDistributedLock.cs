using StackExchange.Redis;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Caching
{
    public sealed class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisDistributedLock> _logger;

        public RedisDistributedLock(IConnectionMultiplexer redis, ILogger<RedisDistributedLock> logger)
        {
            if (redis is null) throw new ArgumentNullException(nameof(redis));
            _db = redis.GetDatabase();
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RedisDistributedLock>.Instance;
        }

        public async Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis lock AcquireAsync called with empty key");
                throw new ArgumentException("Key must not be null or empty", nameof(key));
            }

            if (ttl <= TimeSpan.Zero)
            {
                _logger.LogWarning("Redis lock AcquireAsync called with non-positive TTL for key {Key}", key);
                throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must be greater than zero");
            }

            ct.ThrowIfCancellationRequested();

            var token = Guid.NewGuid().ToString("N");

            try
            {
                var acquired = await _db.StringSetAsync(
                    key,
                    token,
                    ttl,
                    when: When.NotExists);

                if (!acquired)
                {
                    _logger.LogDebug("Redis lock NOT acquired for key {Key}", key);
                    return null;
                }

                _logger.LogDebug("Redis lock acquired for key {Key} with TTL {TtlSeconds}s", key, ttl.TotalSeconds);
                return new RedisLockHandle(_db, key, token, _logger);
            }
            catch (RedisException rex)
            {
                _logger.LogError(rex, "Redis error acquiring lock for key {Key}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error acquiring lock for key {Key}", key);
                throw;
            }
        }

        private sealed class RedisLockHandle : IAsyncDisposable
        {
            private readonly IDatabase _db;
            private readonly string _key;
            private readonly string _token;
            private readonly ILogger _logger;
            private bool _released;

            private const string ReleaseScript =
                "if redis.call('get', KEYS[1]) == ARGV[1] then " +
                "  return redis.call('del', KEYS[1]) " +
                "else return 0 end";

            public RedisLockHandle(IDatabase db, string key, string token, ILogger logger)
            {
                _db = db;
                _key = key;
                _token = token;
                _logger = logger;
            }

            public async ValueTask DisposeAsync()
            {
                if (_released) return;
                _released = true;

                try
                {
                    var result = await _db.ScriptEvaluateAsync(
                        ReleaseScript,
                        keys: new RedisKey[] { _key },
                        values: new RedisValue[] { _token });

                    _logger.LogDebug("Redis lock released for key {Key} (result: {Result})", _key, result.ToString());
                }
                catch (RedisException rex)
                {
                    _logger.LogWarning(rex, "Redis error releasing lock for key {Key} (may have expired)", _key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error releasing lock for key {Key}", _key);
                }
            }
        }
    }
}
