using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    /// <summary>
    /// Valida el id_token de "Sign in with Apple". A diferencia de Google (que tiene
    /// un SDK dedicado), aquí verificamos el JWT a mano contra el JWKS de Apple:
    /// firma RS256 con la clave pública publicada por Apple, emisor, audiencia y
    /// expiración. Las claves se cachean en memoria y se refrescan si rotan.
    /// </summary>
    public sealed class AppleIdTokenValidator : IAppleIdTokenValidator
    {
        private readonly HttpClient _httpClient;
        private readonly AppleAuthSettings _settings;

        // Caché de claves compartida entre instancias (el validador es scoped).
        private static readonly SemaphoreSlim JwksLock = new(1, 1);
        private static IReadOnlyCollection<JsonWebKey>? _cachedKeys;
        private static DateTimeOffset _cachedAt = DateTimeOffset.MinValue;

        public AppleIdTokenValidator(HttpClient httpClient, IOptions<AppleAuthSettings> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = options.Value ?? throw new InvalidOperationException("AppleAuthSettings are missing.");
        }

        public async Task<AppleTokenPayload> ValidateAsync(string idToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new InvalidOperationException("Apple id_token is required.");
            }

            if (_settings.ClientIds.Count == 0)
            {
                throw new InvalidOperationException("AppleAuthSettings.ClientIds is required.");
            }

            var handler = new JwtSecurityTokenHandler();

            // Reintenta una vez forzando refresco del JWKS: cubre el caso en que Apple
            // rotó las claves y la caché aún tiene las viejas.
            JwtSecurityToken? validatedToken = null;
            for (var attempt = 0; attempt < 2 && validatedToken is null; attempt++)
            {
                var keys = await GetSigningKeysAsync(forceRefresh: attempt > 0, cancellationToken);

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudiences = _settings.ClientIds,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = keys,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                };

                try
                {
                    handler.ValidateToken(idToken, parameters, out var securityToken);
                    validatedToken = (JwtSecurityToken)securityToken;
                }
                catch (SecurityTokenSignatureKeyNotFoundException) when (attempt == 0)
                {
                    // La clave no está en la caché: refresca y reintenta.
                }
            }

            if (validatedToken is null)
            {
                throw new SecurityTokenSignatureKeyNotFoundException(
                    "No matching Apple signing key was found for the id_token.");
            }

            var subject = validatedToken.Subject;
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new SecurityTokenException("Apple id_token does not contain a subject.");
            }

            var email = validatedToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var emailVerified = ReadBoolClaim(validatedToken, "email_verified");
            var isPrivateEmail = ReadBoolClaim(validatedToken, "is_private_email");

            return new AppleTokenPayload(subject, email, emailVerified, isPrivateEmail);
        }

        // Apple manda estos flags como string ("true"/"false") o como booleano JSON.
        private static bool ReadBoolClaim(JwtSecurityToken token, string claimType)
        {
            var value = token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
            return bool.TryParse(value, out var parsed) && parsed;
        }

        private async Task<IReadOnlyCollection<JsonWebKey>> GetSigningKeysAsync(
            bool forceRefresh,
            CancellationToken cancellationToken)
        {
            var cacheTtl = TimeSpan.FromMinutes(_settings.JwksCacheMinutes);
            var isFresh = _cachedKeys is not null && DateTimeOffset.UtcNow - _cachedAt < cacheTtl;
            if (!forceRefresh && isFresh)
            {
                return _cachedKeys!;
            }

            await JwksLock.WaitAsync(cancellationToken);
            try
            {
                isFresh = _cachedKeys is not null && DateTimeOffset.UtcNow - _cachedAt < cacheTtl;
                if (!forceRefresh && isFresh)
                {
                    return _cachedKeys!;
                }

                var json = await _httpClient.GetStringAsync(_settings.JwksUri, cancellationToken);
                var keySet = new JsonWebKeySet(json);
                if (keySet.Keys.Count == 0)
                {
                    throw new InvalidOperationException("Apple JWKS endpoint returned no keys.");
                }

                _cachedKeys = keySet.Keys.ToList();
                _cachedAt = DateTimeOffset.UtcNow;
                return _cachedKeys;
            }
            finally
            {
                JwksLock.Release();
            }
        }
    }
}
