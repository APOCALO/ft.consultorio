using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly RefreshTokenSettings _settings;

        public RefreshTokenService(IOptions<RefreshTokenSettings> options)
        {
            _settings = options.Value ?? throw new InvalidOperationException("RefreshTokenSettings are missing.");

            if (_settings.ExpiryDays <= 0)
            {
                throw new InvalidOperationException("RefreshTokenSettings.ExpiryDays must be greater than zero.");
            }

            if (_settings.TokenByteLength < 32)
            {
                throw new InvalidOperationException("RefreshTokenSettings.TokenByteLength must be at least 32 bytes.");
            }
        }

        public RefreshTokenDescriptor CreateToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(_settings.TokenByteLength);
            var token = Base64UrlEncode(tokenBytes);
            var tokenHash = HashToken(token);
            var expiresAt = DateTimeOffset.UtcNow.AddDays(_settings.ExpiryDays);

            return new RefreshTokenDescriptor(token, tokenHash, expiresAt);
        }

        public string HashToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required.", nameof(token));

            var bytes = Encoding.UTF8.GetBytes(token.Trim());
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
