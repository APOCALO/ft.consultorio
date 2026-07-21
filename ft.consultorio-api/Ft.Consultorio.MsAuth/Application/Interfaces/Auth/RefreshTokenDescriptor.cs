namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public sealed record RefreshTokenDescriptor(string Token, string TokenHash, DateTimeOffset ExpiresAt);
}
