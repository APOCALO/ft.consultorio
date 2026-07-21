namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IAppleIdTokenValidator
    {
        /// <summary>
        /// Valida la firma (contra el JWKS de Apple), el emisor, la audiencia y la
        /// expiración del id_token. Lanza si el token no es válido.
        /// </summary>
        Task<AppleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken);
    }
}
