using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;

namespace Ft.Consultorio.MsAuth.Application.Auth
{
    internal static class PasswordResetRequestLimiter
    {
        private static readonly TimeSpan SendWindow = TimeSpan.FromHours(1);
        private static readonly TimeSpan StateTtl = TimeSpan.FromHours(2);

        internal sealed record RequestState(
            DateTimeOffset WindowStartedAt,
            int SentCount,
            DateTimeOffset LastSentAt);

        internal sealed record RequestDecision(
            bool Allowed,
            bool IsCooldown,
            int RetryAfterSeconds)
        {
            public static RequestDecision Allow => new(true, false, 0);
            public static RequestDecision DenyCooldown(int retryAfterSeconds) => new(false, true, retryAfterSeconds);
            public static RequestDecision DenyWindow(int retryAfterSeconds) => new(false, false, retryAfterSeconds);
        }

        public static async Task<RequestDecision> TryConsumeAsync(
            IRedisCacheService cache,
            string email,
            string? requestIp,
            PasswordResetTokenSettings settings,
            ILogger logger)
        {
            try
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                var emailKey = BuildEmailKey(normalizedEmail);
                var emailState = await cache.GetAsync<RequestState>(emailKey);
                var emailDecision = EvaluateState(
                    emailState,
                    settings.MaxRequestsPerHourPerEmail,
                    settings.ResendCooldownSeconds,
                    DateTimeOffset.UtcNow);

                if (!emailDecision.Allowed)
                {
                    return emailDecision;
                }

                RequestState? ipState = null;
                string? ipKey = null;
                RequestDecision ipDecision = RequestDecision.Allow;

                if (!string.IsNullOrWhiteSpace(requestIp))
                {
                    ipKey = BuildIpKey(requestIp);
                    ipState = await cache.GetAsync<RequestState>(ipKey);
                    ipDecision = EvaluateState(
                        ipState,
                        settings.MaxRequestsPerHourPerIp,
                        settings.ResendCooldownSeconds,
                        DateTimeOffset.UtcNow);

                    if (!ipDecision.Allowed)
                    {
                        return ipDecision;
                    }
                }

                var now = DateTimeOffset.UtcNow;
                await cache.SetAsync(emailKey, BuildNextState(emailState, now), StateTtl);

                if (!string.IsNullOrWhiteSpace(ipKey))
                {
                    await cache.SetAsync(ipKey!, BuildNextState(ipState, now), StateTtl);
                }

                return RequestDecision.Allow;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Password reset request limiter cache operation failed. Allowing request.");
                return RequestDecision.Allow;
            }
        }

        private static RequestDecision EvaluateState(
            RequestState? state,
            int maxRequestsPerHour,
            int cooldownSeconds,
            DateTimeOffset now)
        {
            if (state is null)
            {
                return RequestDecision.Allow;
            }

            var cooldownUntil = state.LastSentAt.AddSeconds(cooldownSeconds);
            if (cooldownUntil > now)
            {
                var retryAfter = Math.Max(1, (int)Math.Ceiling((cooldownUntil - now).TotalSeconds));
                return RequestDecision.DenyCooldown(retryAfter);
            }

            var windowEndsAt = state.WindowStartedAt.Add(SendWindow);
            var inWindow = now < windowEndsAt;
            var sentCountInWindow = inWindow ? state.SentCount : 0;

            if (sentCountInWindow >= maxRequestsPerHour)
            {
                var retryAfter = Math.Max(1, (int)Math.Ceiling((windowEndsAt - now).TotalSeconds));
                return RequestDecision.DenyWindow(retryAfter);
            }

            return RequestDecision.Allow;
        }

        private static RequestState BuildNextState(RequestState? current, DateTimeOffset now)
        {
            if (current is null || now >= current.WindowStartedAt.Add(SendWindow))
            {
                return new RequestState(now, 1, now);
            }

            return new RequestState(current.WindowStartedAt, current.SentCount + 1, now);
        }

        private static string BuildEmailKey(string email)
            => $"v1:auth:password-reset:request:email:{email}";

        private static string BuildIpKey(string ip)
        {
            var safeIp = ip.Trim();
            return $"v1:auth:password-reset:request:ip:{safeIp}";
        }
    }
}
