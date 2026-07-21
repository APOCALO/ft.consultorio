namespace Ft.Consultorio.MsAuth.Infrastructure.Settings
{
    public sealed class RefreshTokenSettings
    {
        public int ExpiryDays { get; init; } = 7;
        public int TokenByteLength { get; init; } = 64;
        public int MaxActiveTokensPerUser { get; init; } = 5;
    }
}
