namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public sealed record AuthTokenResult(string AccessToken, DateTimeOffset ExpiresAt);
}
