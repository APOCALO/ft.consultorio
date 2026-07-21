using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ResetPassword
{
    internal sealed class ResetPasswordCommandHandler
        : ApiBaseHandler<ResetPasswordCommand, ResetPasswordResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IPasswordResetTokenService _passwordResetTokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRedisCacheService _cache;
        private readonly PasswordResetTokenSettings _passwordResetSettings;

        public ResetPasswordCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ResetPasswordCommandHandler> logger,
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IPasswordResetTokenService passwordResetTokenService,
            IPasswordHasher<User> passwordHasher,
            IRefreshTokenRepository refreshTokenRepository,
            IRedisCacheService cache,
            IOptions<PasswordResetTokenSettings> passwordResetOptions)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _passwordResetTokenService = passwordResetTokenService ?? throw new ArgumentNullException(nameof(passwordResetTokenService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _passwordResetSettings = passwordResetOptions?.Value ?? throw new ArgumentNullException(nameof(passwordResetOptions));
        }

        protected override async Task<ErrorOr<ApiResponse<ResetPasswordResponseDTO>>> HandleRequest(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var attemptKey = AuthAttemptLimiter.BuildKey("password-reset", "request", request.RequestIp);
            var lockoutError = await AuthAttemptLimiter.CheckLockoutAsync(
                _cache,
                attemptKey,
                _logger,
                "Auth.PasswordReset.Throttled");

            if (lockoutError is not null)
            {
                return lockoutError.Value;
            }

            var tokenHash = _passwordResetTokenService.HashToken(request.Token.Trim());
            var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            if (resetToken is null)
            {
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return InvalidTokenError();
            }

            if (!resetToken.IsActive)
            {
                if (!resetToken.IsUsed)
                {
                    resetToken.RegisterFailedAttempt(_passwordResetSettings.MaxVerificationAttempts);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return InvalidTokenError();
            }

            var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                resetToken.Invalidate();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return InvalidTokenError();
            }

            var newPasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.SetPasswordHash(newPasswordHash);
            resetToken.MarkAsUsed();

            await _passwordResetTokenRepository.InvalidateActiveByUserIdAsync(user.Id, cancellationToken);

            var activeRefreshTokens = await _refreshTokenRepository.GetActiveTokensByUserAsync(user.Id, cancellationToken);
            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.Revoke(
                    reason: "Password was reset.",
                    revokedByIp: request.RequestIp,
                    replacedByTokenHash: null);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var refreshToken in activeRefreshTokens)
            {
                await RefreshTokenCache.TrySetAsync(
                    _cache,
                    refreshToken.TokenHash,
                    new RefreshTokenCacheEntry(
                        refreshToken.Id,
                        refreshToken.UserId,
                        refreshToken.ExpiresAt,
                        refreshToken.RevokedAt),
                    refreshToken.ExpiresAt,
                    _logger);
            }

            await AuthAttemptLimiter.ClearAsync(_cache, attemptKey, _logger);

            var response = new ResetPasswordResponseDTO
            {
                Success = true,
                Message = "Tu contraseña fue actualizada correctamente."
            };

            return new ApiResponse<ResetPasswordResponseDTO>(response, true);
        }

        private static Error InvalidTokenError()
            => Error.Unauthorized(
                "Auth.PasswordReset.InvalidOrExpired",
                "Invalid or expired password reset token.");
    }
}
