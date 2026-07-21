namespace Ft.Consultorio.MsAuth.Application.Auth.DTOs
{
    public sealed class RegisterLocalUserPendingResponseDTO
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = default!;
        public bool RequiresEmailVerification { get; init; } = true;
        public int OtpLength { get; init; }
        public int OtpExpiresInMinutes { get; init; }
        public int ResendCooldownSeconds { get; init; }
    }
}
