using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    public sealed class GoogleIdTokenValidator : IGoogleIdTokenValidator
    {
        private readonly GoogleAuthSettings _settings;

        public GoogleIdTokenValidator(IOptions<GoogleAuthSettings> options)
        {
            _settings = options.Value ?? throw new InvalidOperationException("GoogleAuthSettings are missing.");
        }

        public async Task<GoogleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new InvalidOperationException("Google id_token is required.");
            }

            if (_settings.ClientIds.Count == 0)
            {
                throw new InvalidOperationException("GoogleAuthSettings.ClientIds is required.");
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _settings.ClientIds
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleTokenPayload(
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.GivenName,
                payload.FamilyName,
                payload.Name,
                payload.Picture);
        }
    }
}
