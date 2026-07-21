using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsAuth.Application.Auth;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Clients;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RegisterLocalUser
{
    internal sealed class RegisterLocalUserCommandHandler
        : ApiBaseHandler<RegisterLocalUserCommand, RegisterLocalUserPendingResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEmailVerificationOtpRepository _emailVerificationOtpRepository;
        private readonly IEmailVerificationOtpService _emailVerificationOtpService;
        private readonly EmailVerificationOtpSettings _emailVerificationOtpSettings;
        private readonly IRedisCacheService _cache;
        private readonly INotificationsApiClient _notifications;
        private readonly ITurnstileVerifier _turnstileVerifier;

        public RegisterLocalUserCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<RegisterLocalUserCommandHandler> logger,
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IEmailVerificationOtpRepository emailVerificationOtpRepository,
            IEmailVerificationOtpService emailVerificationOtpService,
            IOptions<EmailVerificationOtpSettings> emailVerificationOtpOptions,
            IRedisCacheService cache,
            INotificationsApiClient notifications,
            ITurnstileVerifier turnstileVerifier)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _emailVerificationOtpRepository = emailVerificationOtpRepository ?? throw new ArgumentNullException(nameof(emailVerificationOtpRepository));
            _emailVerificationOtpService = emailVerificationOtpService ?? throw new ArgumentNullException(nameof(emailVerificationOtpService));
            _emailVerificationOtpSettings = emailVerificationOtpOptions?.Value ?? throw new ArgumentNullException(nameof(emailVerificationOtpOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            _turnstileVerifier = turnstileVerifier ?? throw new ArgumentNullException(nameof(turnstileVerifier));
        }

        protected override async Task<ErrorOr<ApiResponse<RegisterLocalUserPendingResponseDTO>>> HandleRequest(
            RegisterLocalUserCommand request,
            CancellationToken cancellationToken)
        {
            // Verificación del captcha antes de tocar la base de datos o el cache.
            var captchaValid = await _turnstileVerifier.VerifyAsync(
                request.CaptchaToken,
                request.RequestIp,
                cancellationToken);

            if (!captchaValid)
            {
                return Error.Forbidden("Auth.Register.CaptchaInvalid", "Captcha verification failed.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var attemptKey = AuthAttemptLimiter.BuildKey("register", normalizedEmail, request.RequestIp);
            var lockoutError = await AuthAttemptLimiter.CheckLockoutAsync(
                _cache,
                attemptKey,
                _logger,
                "Auth.Register.Throttled");

            if (lockoutError is not null)
            {
                return lockoutError.Value;
            }

            var existing = await _userRepository.GetByEmailAsync(normalizedEmail, asNoTracking: true, cancellationToken);
            if (existing is not null)
            {
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return Error.Conflict("Auth.Register.Email", "Email is already registered.");
            }

            var userId = Guid.NewGuid();
            var userName = string.IsNullOrWhiteSpace(request.UserName)
                ? normalizedEmail.Split('@')[0]
                : request.UserName.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = $"user{userId.ToString("N")[..8]}";
            }

            var user = User.Create(
                createdById: userId,
                authUserId: $"local:{userId}",
                userName: userName,
                userEmail: normalizedEmail,
                firstName: request.FirstName,
                lastName: request.LastName,
                birthDate: request.BirthDate,
                avatarUrl: request.AvatarUrl,
                isActive: false,
                isEmailVerified: false,
                settings: UserSettings.Create(userId, null, null, null),
                id: userId);

            var passwordHash = _passwordHasher.HashPassword(user, request.Password);
            user.SetPasswordHash(passwordHash);

            var otpDescriptor = _emailVerificationOtpService.CreateOtp();
            var otp = EmailVerificationOtp.Create(
                userId: user.Id,
                email: user.UserEmail,
                codeHash: otpDescriptor.CodeHash,
                codeSalt: otpDescriptor.CodeSalt,
                expiresAt: otpDescriptor.ExpiresAt);

            await _userRepository.AddAsync(user, cancellationToken);
            await _emailVerificationOtpRepository.AddAsync(otp, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await AuthAttemptLimiter.ClearAsync(_cache, attemptKey, _logger);
            await EmailVerificationOtpResendLimiter.TryConsumeAsync(
                _cache,
                normalizedEmail,
                request.RequestIp,
                _emailVerificationOtpSettings,
                _logger);

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
                    "Failed to dispatch OTP email for user {UserId} ({Email}).",
                    user.Id,
                    user.UserEmail);
            }

            var response = new RegisterLocalUserPendingResponseDTO
            {
                UserId = user.Id,
                Email = user.UserEmail,
                RequiresEmailVerification = true,
                OtpLength = _emailVerificationOtpSettings.CodeLength,
                OtpExpiresInMinutes = _emailVerificationOtpSettings.TtlMinutes,
                ResendCooldownSeconds = _emailVerificationOtpSettings.ResendCooldownSeconds
            };

            return new ApiResponse<RegisterLocalUserPendingResponseDTO>(response, true);
        }
    }
}
