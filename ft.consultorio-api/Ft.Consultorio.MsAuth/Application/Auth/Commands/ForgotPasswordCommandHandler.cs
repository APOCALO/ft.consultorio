using ErrorOr;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Clients;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ForgotPassword
{
    internal sealed class ForgotPasswordCommandHandler
        : ApiBaseHandler<ForgotPasswordCommand, ForgotPasswordResponseDTO>
    {
        private const string GenericMessage = "Si existe una cuenta, te enviaremos un enlace para restablecer tu contraseña.";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IPasswordResetTokenService _passwordResetTokenService;
        private readonly PasswordResetTokenSettings _passwordResetSettings;
        private readonly INotificationsApiClient _notifications;
        private readonly IRedisCacheService _cache;
        private readonly IConfiguration _configuration;

        public ForgotPasswordCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ForgotPasswordCommandHandler> logger,
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IPasswordResetTokenService passwordResetTokenService,
            IOptions<PasswordResetTokenSettings> passwordResetOptions,
            INotificationsApiClient notifications,
            IRedisCacheService cache,
            IConfiguration configuration)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _passwordResetTokenService = passwordResetTokenService ?? throw new ArgumentNullException(nameof(passwordResetTokenService));
            _passwordResetSettings = passwordResetOptions?.Value ?? throw new ArgumentNullException(nameof(passwordResetOptions));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task<ErrorOr<ApiResponse<ForgotPasswordResponseDTO>>> HandleRequest(
            ForgotPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var genericResponse = new ForgotPasswordResponseDTO
            {
                Message = GenericMessage
            };

            try
            {
                var normalizedEmail = request.Email.Trim().ToLowerInvariant();

                var limitDecision = await PasswordResetRequestLimiter.TryConsumeAsync(
                    _cache,
                    normalizedEmail,
                    request.RequestIp,
                    _passwordResetSettings,
                    _logger);

                if (!limitDecision.Allowed)
                {
                    _logger.LogInformation(
                        "Forgot password throttled for email {Email}. Cooldown={IsCooldown} RetryAfterSeconds={RetryAfter}",
                        normalizedEmail,
                        limitDecision.IsCooldown,
                        limitDecision.RetryAfterSeconds);

                    await ApplyMinimumDelayAsync(startedAt, cancellationToken);
                    return new ApiResponse<ForgotPasswordResponseDTO>(genericResponse, true);
                }

                var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
                if (user is null
                    || string.IsNullOrWhiteSpace(user.PasswordHash)
                    || !user.IsActive
                    || !user.IsEmailVerified)
                {
                    await ApplyMinimumDelayAsync(startedAt, cancellationToken);
                    return new ApiResponse<ForgotPasswordResponseDTO>(genericResponse, true);
                }

                var tokenDescriptor = _passwordResetTokenService.CreateToken();
                await _passwordResetTokenRepository.InvalidateActiveByUserIdAsync(user.Id, cancellationToken);

                var resetToken = PasswordResetToken.Create(
                    userId: user.Id,
                    tokenHash: tokenDescriptor.TokenHash,
                    expiresAt: tokenDescriptor.ExpiresAt,
                    createdByIp: request.RequestIp,
                    createdByUserAgent: request.UserAgent);

                await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var resetLink = BuildResetLink(tokenDescriptor.Token);

                try
                {
                    await _notifications.SendPasswordResetLinkAsync(
                        to: user.UserEmail,
                        firstName: user.FirstName,
                        resetLink: resetLink,
                        expiresInMinutes: _passwordResetSettings.TtlMinutes,
                        ct: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to dispatch password reset email for user {UserId} ({Email}).",
                        user.Id,
                        user.UserEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password flow failed. Returning generic response.");
            }

            await ApplyMinimumDelayAsync(startedAt, cancellationToken);
            return new ApiResponse<ForgotPasswordResponseDTO>(genericResponse, true);
        }

        private string BuildResetLink(string token)
        {
            var configuredBaseUrl = _configuration["FRONTEND_BASE_URL"];
            var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? _passwordResetSettings.FrontendBaseUrl
                : configuredBaseUrl;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                baseUri = new Uri("http://localhost:3000");
            }

            var builder = new UriBuilder(baseUri)
            {
                Path = "/auth/reset-password",
                Query = $"token={Uri.EscapeDataString(token)}"
            };

            return builder.Uri.ToString();
        }

        private async Task ApplyMinimumDelayAsync(DateTimeOffset startedAt, CancellationToken cancellationToken)
        {
            if (_passwordResetSettings.MinimumResponseDelayMilliseconds <= 0)
            {
                return;
            }

            var elapsed = DateTimeOffset.UtcNow - startedAt;
            var minimum = TimeSpan.FromMilliseconds(_passwordResetSettings.MinimumResponseDelayMilliseconds);
            var remaining = minimum - elapsed;

            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, cancellationToken);
            }
        }
    }
}
