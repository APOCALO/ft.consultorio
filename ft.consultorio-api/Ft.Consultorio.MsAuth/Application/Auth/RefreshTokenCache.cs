using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Microsoft.Extensions.Logging;

namespace Ft.Consultorio.MsAuth.Application.Auth
{
    internal static class RefreshTokenCache
    {
        private const string Prefix = "auth:refresh:";

        public static string BuildKey(string tokenHash) => $"{Prefix}{tokenHash}";

        public static async Task<RefreshTokenCacheEntry?> TryGetAsync(
            IRedisCacheService cache,
            string tokenHash,
            ILogger logger)
        {
            try
            {
                return await cache.GetAsync<RefreshTokenCacheEntry>(BuildKey(tokenHash));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Refresh token cache GET failed.");
                return null;
            }
        }

        public static async Task TrySetAsync(
            IRedisCacheService cache,
            string tokenHash,
            RefreshTokenCacheEntry entry,
            DateTimeOffset expiresAt,
            ILogger logger)
        {
            try
            {
                var ttl = expiresAt - DateTimeOffset.UtcNow;
                if (ttl <= TimeSpan.Zero)
                {
                    return;
                }

                await cache.SetAsync(BuildKey(tokenHash), entry, ttl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Refresh token cache SET failed.");
            }
        }

        public static async Task TryRemoveAsync(
            IRedisCacheService cache,
            string tokenHash,
            ILogger logger)
        {
            try
            {
                await cache.RemoveAsync(BuildKey(tokenHash));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Refresh token cache DEL failed.");
            }
        }
    }
}
