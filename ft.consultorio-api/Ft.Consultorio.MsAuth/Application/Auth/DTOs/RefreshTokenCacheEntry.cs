namespace Ft.Consultorio.MsAuth.Application.Auth.DTOs
{
    public sealed record RefreshTokenCacheEntry(
        Guid Id,
        Guid UserId,
        DateTimeOffset ExpiresAt,
        DateTimeOffset? RevokedAt);
}
