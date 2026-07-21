using Ft.Consultorio.MsAuth.Application.Users.DTOs;

namespace Ft.Consultorio.MsAuth.Application.Auth.DTOs
{
    public sealed class AuthTokenResponseDTO
    {
        public string AccessToken { get; init; } = default!;
        public string TokenType { get; init; } = "Bearer";
        public DateTimeOffset ExpiresAt { get; init; }
        public string RefreshToken { get; init; } = default!;
        public DateTimeOffset RefreshTokenExpiresAt { get; init; }
        public bool IsFirstLogin { get; init; }
        public UserSessionResponseDTO User { get; init; } = default!;
    }
}

