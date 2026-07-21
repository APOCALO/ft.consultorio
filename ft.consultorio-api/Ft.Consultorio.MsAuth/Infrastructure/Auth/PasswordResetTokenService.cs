using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    public sealed class PasswordResetTokenService : IPasswordResetTokenService
    {
        private readonly PasswordResetTokenSettings _settings;
        private readonly IRefreshTokenService _refreshTokenService;

        public PasswordResetTokenService(
            IOptions<PasswordResetTokenSettings> options,
            IRefreshTokenService refreshTokenService)
        {
            _settings = options?.Value ?? throw new InvalidOperationException("PasswordResetTokenSettings are missing.");
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));

            if (_settings.TtlMinutes <= 0 || _settings.TtlMinutes > 60)
            {
                throw new InvalidOperationException("Auth:PasswordReset:TtlMinutes must be between 1 and 60.");
            }

            if (_settings.TokenByteLength < 32)
            {
                throw new InvalidOperationException("Auth:PasswordReset:TokenByteLength must be at least 32 bytes.");
            }

            if (_settings.MaxVerificationAttempts <= 0)
            {
                throw new InvalidOperationException("Auth:PasswordReset:MaxVerificationAttempts must be greater than zero.");
            }

            if (_settings.ResendCooldownSeconds <= 0)
            {
                throw new InvalidOperationException("Auth:PasswordReset:ResendCooldownSeconds must be greater than zero.");
            }

            if (_settings.MaxRequestsPerHourPerEmail <= 0)
            {
                throw new InvalidOperationException("Auth:PasswordReset:MaxRequestsPerHourPerEmail must be greater than zero.");
            }

            if (_settings.MaxRequestsPerHourPerIp <= 0)
            {
                throw new InvalidOperationException("Auth:PasswordReset:MaxRequestsPerHourPerIp must be greater than zero.");
            }

            if (_settings.MinimumResponseDelayMilliseconds < 0)
            {
                throw new InvalidOperationException("Auth:PasswordReset:MinimumResponseDelayMilliseconds cannot be negative.");
            }
        }

        public PasswordResetTokenDescriptor CreateToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(_settings.TokenByteLength);
            var token = Base64UrlEncode(tokenBytes);
            var tokenHash = _refreshTokenService.HashToken(token);
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.TtlMinutes);

            return new PasswordResetTokenDescriptor(token, tokenHash, expiresAt);
        }

        public string HashToken(string token) => _refreshTokenService.HashToken(token);

        private static string Base64UrlEncode(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
