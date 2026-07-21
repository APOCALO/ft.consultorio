namespace Ft.Consultorio.MsAuth.Application.Auth.DTOs
{
    public sealed class ResendEmailOtpResponseDTO
    {
        public bool Accepted { get; init; } = true;
        public int ResendCooldownSeconds { get; init; }
    }
}
