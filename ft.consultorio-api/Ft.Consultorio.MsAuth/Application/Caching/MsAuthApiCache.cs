using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using System.Globalization;

namespace Ft.Consultorio.MsAuth.Application.Caching
{
    internal static class MsAuthApiCache
    {
        private const string UsersVersionKey = "msauth:user:version:v1";
        private const string AvatarsVersionKey = "msauth:avatar:version:v1";
        public static readonly TimeSpan UserReadTtl = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan RoleReadTtl = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan AvatarReadTtl = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan AvatarCategoriesTotl = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan UsersVersionTtl = TimeSpan.FromDays(30);
        private static readonly TimeSpan AvatarsVersionTtl = TimeSpan.FromDays(30);

        public static string BuildUserByIdKey(Guid userId, long version)
            => $"msauth:user:by-id:v1:{version.ToString(CultureInfo.InvariantCulture)}:{userId:D}";

        public static string BuildUserByAuthUserIdKey(string authUserId, long version)
            => $"msauth:user:by-auth-id:v1:{version.ToString(CultureInfo.InvariantCulture)}:{authUserId.Trim()}";

        public static string BuildUserSettingsByUserIdKey(Guid userId, long version)
            => $"msauth:user-settings:by-user-id:v1:{version.ToString(CultureInfo.InvariantCulture)}:{userId:D}";

        public static string BuildRoleByIdKey(Guid roleId)
            => $"msauth:role:by-id:v1:{roleId:D}";

        public static string BuildAvatarByIdKey(Guid avatarId, long version)
            => $"msauth:avatar:by-id:v1:{version.ToString(CultureInfo.InvariantCulture)}:{avatarId:D}";

        public static string BuildAvatarsPagedKey(PaginationParameters pagination, string? category, bool activeOnly, string? search, long version)
            => $"msauth:avatar:paged:v1:{version.ToString(CultureInfo.InvariantCulture)}:page:{pagination.PageNumber}:size:{pagination.PageSize}:active:{activeOnly}:cat:{category ?? "_"}:q:{search ?? "_"}";

        public static string BuildAvatarCategoriesKey(bool activeOnly, long version)
            => $"msauth:avatar:categories:v1:{version.ToString(CultureInfo.InvariantCulture)}:active:{activeOnly}";

        public static async Task<T?> TryGetAsync<T>(
            IRedisCacheService cache,
            string key,
            ILogger logger)
        {
            try
            {
                return await cache.GetAsync<T>(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis GET failed for key {CacheKey}. Continuing without cache.", key);
                return default;
            }
        }

        public static async Task TrySetAsync<T>(
            IRedisCacheService cache,
            string key,
            T payload,
            TimeSpan ttl,
            ILogger logger)
        {
            try
            {
                await cache.SetAsync(key, payload, ttl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis SET failed for key {CacheKey}. Continuing without cache.", key);
            }
        }

        public static async Task InvalidateUserAsync(
            IRedisCacheService cache,
            Guid userId,
            string? authUserId,
            ILogger logger)
        {
            var version = await GetUsersVersionAsync(cache, logger);
            await TryRemoveAsync(cache, BuildUserByIdKey(userId, version), logger);
            await TryRemoveAsync(cache, BuildUserSettingsByUserIdKey(userId, version), logger);

            if (!string.IsNullOrWhiteSpace(authUserId))
            {
                await TryRemoveAsync(cache, BuildUserByAuthUserIdKey(authUserId, version), logger);
            }
        }

        public static async Task InvalidateRoleAsync(
            IRedisCacheService cache,
            Guid roleId,
            ILogger logger)
        {
            await TryRemoveAsync(cache, BuildRoleByIdKey(roleId), logger);
        }

        public static async Task InvalidateAllUsersAsync(
            IRedisCacheService cache,
            ILogger logger)
        {
            // Atomic INCR — see MsBookingsApiCache.BumpVersionAsync for rationale.
            await BumpUsersVersionAsync(cache, logger);
        }

        public static async Task<long> GetUsersVersionAsync(
            IRedisCacheService cache,
            ILogger logger)
        {
            try
            {
                // Cold-cache safe read: see MsBookingsApiCache.GetVersionAsync for rationale.
                var version = await cache.GetAsync<long>(UsersVersionKey);
                return version > 0 ? version : await cache.IncrementAsync(UsersVersionKey, UsersVersionTtl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis GET failed for version key {CacheKey}. Falling back to default.", UsersVersionKey);
                return 0L;
            }
        }

        public static async Task<long> GetAvatarsVersionAsync(
            IRedisCacheService cache,
            ILogger logger)
        {
            try
            {
                // Cold-cache safe read: see MsBookingsApiCache.GetVersionAsync for rationale.
                var version = await cache.GetAsync<long>(AvatarsVersionKey);
                return version > 0 ? version : await cache.IncrementAsync(AvatarsVersionKey, AvatarsVersionTtl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis GET failed for version key {CacheKey}. Falling back to default.", AvatarsVersionKey);
                return 0L;
            }
        }

        public static async Task InvalidateAvatarAsync(
            IRedisCacheService cache,
            Guid avatarId,
            ILogger logger)
        {
            var version = await GetAvatarsVersionAsync(cache, logger);
            await TryRemoveAsync(cache, BuildAvatarByIdKey(avatarId, version), logger);
            await BumpAvatarsVersionAsync(cache, logger);
        }

        public static async Task InvalidateAllAvatarsAsync(
            IRedisCacheService cache,
            ILogger logger)
        {
            await BumpAvatarsVersionAsync(cache, logger);
        }

        private static async Task BumpUsersVersionAsync(
            IRedisCacheService cache,
            ILogger logger)
        {
            try
            {
                // Atomic INCR — see MsBookingsApiCache.BumpVersionAsync for rationale.
                await cache.IncrementAsync(UsersVersionKey, UsersVersionTtl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis version bump failed for key {CacheKey}. Continuing without cache.", UsersVersionKey);
            }
        }

        private static async Task BumpAvatarsVersionAsync(
            IRedisCacheService cache,
            ILogger logger)
        {
            try
            {
                // Atomic INCR — see MsBookingsApiCache.BumpVersionAsync for rationale.
                await cache.IncrementAsync(AvatarsVersionKey, AvatarsVersionTtl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis version bump failed for key {CacheKey}. Continuing without cache.", AvatarsVersionKey);
            }
        }

        private static async Task TryRemoveAsync(
            IRedisCacheService cache,
            string key,
            ILogger logger)
        {
            try
            {
                await cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis DEL failed for key {CacheKey}. Continuing without cache.", key);
            }
        }
    }
}
