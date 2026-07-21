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

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ResendEmailOtp
{
    internal sealed class ResendEmailOtpCommandHandler
        : ApiBaseHandler<ResendEmailOtpCommand, ResendEmailOtpResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationOtpRepository _emailVerificationOtpRepository;
        private readonly IEmailVerificationOtpService _emailVerificationOtpService;
        private readonly EmailVerificationOtpSettings _emailVerificationOtpSettings;
        private readonly IRedisCacheService _cache;
        private readonly INotificationsApiClient _notifications;

        public ResendEmailOtpCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ResendEmailOtpCommandHandler> logger,
            IUserRepository userRepository,
            IEmailVerificationOtpRepository emailVerificationOtpRepository,
            IEmailVerificationOtpService emailVerificationOtpService,
            IOptions<EmailVerificationOtpSettings> emailVerificationOtpOptions,
            IRedisCacheService cache,
            INotificationsApiClient notifications)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailVerificationOtpRepository = emailVerificationOtpRepository ?? throw new ArgumentNullException(nameof(emailVerificationOtpRepository));
            _emailVerificationOtpService = emailVerificationOtpService ?? throw new ArgumentNullException(nameof(emailVerificationOtpService));
            _emailVerificationOtpSettings = emailVerificationOtpOptions?.Value ?? throw new ArgumentNullException(nameof(emailVerificationOtpOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        }

        protected override async Task<ErrorOr<ApiResponse<ResendEmailOtpResponseDTO>>> HandleRequest(
            ResendEmailOtpCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var genericResponse = new ResendEmailOtpResponseDTO
            {
                Accepted = true,
                ResendCooldownSeconds = _emailVerificationOtpSettings.ResendCooldownSeconds
            };

            var limitDecision = await EmailVerificationOtpResendLimiter.TryConsumeAsync(
                _cache,
                normalizedEmail,
                request.RequestIp,
                _emailVerificationOtpSettings,
                _logger);

            if (!limitDecision.Allowed)
            {
                _logger.LogInformation(
                    "OTP resend throttled for email {Email}. Cooldown={IsCooldown} RetryAfterSeconds={RetryAfter}",
                    normalizedEmail,
                    limitDecision.IsCooldown,
                    limitDecision.RetryAfterSeconds);

                return new ApiResponse<ResendEmailOtpResponseDTO>(genericResponse, true);
            }

            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (user is null || user.IsEmailVerified || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return new ApiResponse<ResendEmailOtpResponseDTO>(genericResponse, true);
            }

            var otpDescriptor = _emailVerificationOtpService.CreateOtp();

            await _emailVerificationOtpRepository.InvalidateActiveByUserIdAsync(user.Id, cancellationToken);

            var otp = EmailVerificationOtp.Create(
                userId: user.Id,
                email: user.UserEmail,
                codeHash: otpDescriptor.CodeHash,
                codeSalt: otpDescriptor.CodeSalt,
                expiresAt: otpDescriptor.ExpiresAt);

            await _emailVerificationOtpRepository.AddAsync(otp, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _notifications.SendEmailVerificationOtpAsync(
                    to: user.UserEmail,
                    firstName: user.FirstName,
                    otpCode: otpDescriptor.Code,
                    expiresInMinutes: _emailVerificationOtpSettings.TtlMinutes,
                    ct: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to dispatch resent OTP email for user {UserId} ({Email}).",
                    user.Id,
                    user.UserEmail);
            }

            return new ApiResponse<ResendEmailOtpResponseDTO>(genericResponse, true);
        }
    }
}
