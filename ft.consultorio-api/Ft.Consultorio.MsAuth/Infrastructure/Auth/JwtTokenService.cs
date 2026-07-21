using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private static readonly JwtSecurityTokenHandler TokenHandler = new();
        private readonly JwtSettings _settings;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value ?? throw new InvalidOperationException("JwtSettings are missing.");
            _signingCredentials = BuildSigningCredentials(_settings);
        }

        private static SigningCredentials BuildSigningCredentials(JwtSettings settings)
        {
            // Preferido: firma asimétrica RS256. Solo este servicio (el emisor) tiene la
            // clave privada; el resto de la plataforma valida con la pública.
            if (!string.IsNullOrWhiteSpace(settings.PrivateKeyPem))
            {
                var rsa = System.Security.Cryptography.RSA.Create();
                try
                {
                    rsa.ImportFromPem(settings.PrivateKeyPem);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException(
                        "JwtSettings.PrivateKeyPem must be a valid PEM-encoded RSA private key. " +
                        "Generate one with: openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048", ex);
                }

                return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
            }

            // Legado/pruebas: firma simétrica HS256.
            if (string.IsNullOrWhiteSpace(settings.SecretKey))
            {
                throw new InvalidOperationException(
                    "JwtSettings requires PrivateKeyPem (preferred, RS256) or SecretKey (legacy, HS256).");
            }

            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(settings.SecretKey);
            }
            catch (FormatException)
            {
                keyBytes = System.Text.Encoding.UTF8.GetBytes(settings.SecretKey);
            }

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    $"JwtSettings.SecretKey must decode to at least 32 bytes (256 bits) for HS256. " +
                    $"Current key decodes to {keyBytes.Length} bytes.");
            }

            return new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        }

        public AuthTokenResult CreateToken(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(_settings.ExpiryMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.UserEmail),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new(JwtRegisteredClaimNames.Name, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("auth_user_id", user.AuthUserId)
            };

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                claims.Add(new Claim("picture", user.AvatarUrl));
            }

            foreach (var role in user.Roles.Select(r => r.Role.Name).Distinct())
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expires.UtcDateTime,
                signingCredentials: _signingCredentials);

            var tokenValue = TokenHandler.WriteToken(token);
            return new AuthTokenResult(tokenValue, expires);
        }
    }
}
