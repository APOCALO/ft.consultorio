namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public sealed record GoogleTokenPayload(
        string Subject,
        string Email,
        bool EmailVerified,
        string? GivenName,
        string? FamilyName,
        string? FullName,
        string? PictureUrl);
}
