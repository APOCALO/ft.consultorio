namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IGoogleIdTokenValidator
    {
        Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken);
    }
}
