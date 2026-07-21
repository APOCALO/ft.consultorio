using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;

namespace Ft.Consultorio.MsAuth.Application.Auth
{
    internal static class AuthAttemptLimiter
    {
        private const int MaxFailuresBeforeLock = 5;
        private const int BaseLockSeconds = 30;
        private const int MaxLockSeconds = 15 * 60;
        private static readonly TimeSpan StateTtl = TimeSpan.FromHours(2);

        internal sealed record AttemptState(int FailCount, DateTimeOffset? LockedUntil);

        public static string BuildKey(string flow, string email, string? ip)
        {
            var safeEmail = email.Trim().ToLowerInvariant();
            var safeIp = string.IsNullOrWhiteSpace(ip) ? "unknown" : ip.Trim();
            return $"v1:auth:attempts:{flow}:{safeEmail}:{safeIp}";
        }

        public static async Task<Error?> CheckLockoutAsync(
            IRedisCacheService cache,
            string key,
            ILogger logger,
            string errorCode)
        {
            try
            {
                var state = await cache.GetAsync<AttemptState>(key);
                if (state?.LockedUntil is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
                {
                    var seconds = (int)Math.Ceiling((lockedUntil - DateTimeOffset.UtcNow).TotalSeconds);
                    var message = seconds > 0
                        ? $"Too many attempts. Try again in {seconds} seconds."
                        : "Too many attempts. Try again later.";

                    return Error.Forbidden(errorCode, message);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Auth attempt cache lookup failed.");
            }

            return null;
        }

        public static async Task RecordFailureAsync(
            IRedisCacheService cache,
            string key,
            ILogger logger)
        {
            try
            {
                var state = await cache.GetAsync<AttemptState>(key) ?? new AttemptState(0, null);
                var nextCount = state.FailCount + 1;

                DateTimeOffset? lockedUntil = null;
                if (nextCount > MaxFailuresBeforeLock)
                {
                    var exponent = Math.Max(0, (nextCount - MaxFailuresBeforeLock - 1) / 2);
                    var lockSeconds = Math.Min(MaxLockSeconds, BaseLockSeconds * (int)Math.Pow(2, exponent));
                    lockedUntil = DateTimeOffset.UtcNow.AddSeconds(lockSeconds);
                }

                var nextState = new AttemptState(nextCount, lockedUntil);
                await cache.SetAsync(key, nextState, StateTtl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Auth attempt cache update failed.");
            }
        }

        public static async Task ClearAsync(IRedisCacheService cache, string key, ILogger logger)
        {
            try
            {
                await cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Auth attempt cache clear failed.");
            }
        }
    }
}
