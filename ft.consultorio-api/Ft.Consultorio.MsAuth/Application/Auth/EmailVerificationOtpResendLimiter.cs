using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Application.Auth
{
    internal static class EmailVerificationOtpResendLimiter
    {
        private static readonly TimeSpan SendWindow = TimeSpan.FromHours(1);
        private static readonly TimeSpan StateTtl = TimeSpan.FromHours(2);

        internal sealed record ResendState(
            DateTimeOffset WindowStartedAt,
            int SentCount,
            DateTimeOffset LastSentAt);

        internal sealed record ResendDecision(
            bool Allowed,
            bool IsCooldown,
            int RetryAfterSeconds)
        {
            public static ResendDecision Allow => new(true, false, 0);
            public static ResendDecision DenyCooldown(int retryAfterSeconds) => new(false, true, retryAfterSeconds);
            public static ResendDecision DenyWindow(int retryAfterSeconds) => new(false, false, retryAfterSeconds);
        }

        public static async Task<ResendDecision> TryConsumeAsync(
            IRedisCacheService cache,
            string email,
            string? requestIp,
            EmailVerificationOtpSettings settings,
            ILogger logger)
        {
            try
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                var emailKey = BuildEmailKey(normalizedEmail);
                var emailState = await cache.GetAsync<ResendState>(emailKey);
                var emailDecision = EvaluateState(emailState, settings, DateTimeOffset.UtcNow);

                if (!emailDecision.Allowed)
                {
                    return emailDecision;
                }

                ResendState? ipState = null;
                string? ipKey = null;
                ResendDecision ipDecision = ResendDecision.Allow;

                if (!string.IsNullOrWhiteSpace(requestIp))
                {
                    ipKey = BuildIpKey(requestIp);
                    ipState = await cache.GetAsync<ResendState>(ipKey);
                    ipDecision = EvaluateState(ipState, settings, DateTimeOffset.UtcNow);
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

                return ResendDecision.Allow;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "OTP resend limiter cache operation failed. Allowing request.");
                return ResendDecision.Allow;
            }
        }

        private static ResendDecision EvaluateState(
            ResendState? state,
            EmailVerificationOtpSettings settings,
            DateTimeOffset now)
        {
            if (state is null)
            {
                return ResendDecision.Allow;
            }

            var cooldownUntil = state.LastSentAt.AddSeconds(settings.ResendCooldownSeconds);
            if (cooldownUntil > now)
            {
                var retryAfter = Math.Max(1, (int)Math.Ceiling((cooldownUntil - now).TotalSeconds));
                return ResendDecision.DenyCooldown(retryAfter);
            }

            var windowEndsAt = state.WindowStartedAt.Add(SendWindow);
            var inWindow = now < windowEndsAt;
            var sentCountInWindow = inWindow ? state.SentCount : 0;

            if (sentCountInWindow >= settings.MaxSendsPerHourPerEmail)
            {
                var retryAfter = Math.Max(1, (int)Math.Ceiling((windowEndsAt - now).TotalSeconds));
                return ResendDecision.DenyWindow(retryAfter);
            }

            return ResendDecision.Allow;
        }

        private static ResendState BuildNextState(ResendState? current, DateTimeOffset now)
        {
            if (current is null || now >= current.WindowStartedAt.Add(SendWindow))
            {
                return new ResendState(now, 1, now);
            }

            return new ResendState(current.WindowStartedAt, current.SentCount + 1, now);
        }

        private static string BuildEmailKey(string email)
            => $"v1:auth:email-otp:resend:email:{email}";

        private static string BuildIpKey(string ip)
        {
            var safeIp = ip.Trim();
            return $"v1:auth:email-otp:resend:ip:{safeIp}";
        }
    }
}
