namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public sealed record PasswordResetTokenDescriptor(
        string Token,
        string TokenHash,
        DateTimeOffset ExpiresAt);
}
