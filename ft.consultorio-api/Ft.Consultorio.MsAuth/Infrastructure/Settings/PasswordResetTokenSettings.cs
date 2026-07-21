namespace Ft.Consultorio.MsAuth.Infrastructure.Settings
{
    public sealed class PasswordResetTokenSettings
    {
        public int TtlMinutes { get; init; } = 30;
        public int TokenByteLength { get; init; } = 48;
        public int MaxVerificationAttempts { get; init; } = 5;
        public int ResendCooldownSeconds { get; init; } = 60;
        public int MaxRequestsPerHourPerEmail { get; init; } = 5;
        public int MaxRequestsPerHourPerIp { get; init; } = 20;
        public int MinimumResponseDelayMilliseconds { get; init; } = 300;
        public string FrontendBaseUrl { get; init; } = "http://localhost:3000";
    }
}
