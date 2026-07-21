using Ft.Consultorio.MsAuth.Application.Mapping;
using ErrorOr;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Auth;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Caching;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Application.Users.DTOs;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using RefreshTokenEntity = Ft.Consultorio.MsAuth.Domain.RefreshToken;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.VerifyEmailOtp
{
    internal sealed class VerifyEmailOtpCommandHandler
        : ApiBaseHandler<VerifyEmailOtpCommand, AuthTokenResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationOtpRepository _emailVerificationOtpRepository;
        private readonly IEmailVerificationOtpService _emailVerificationOtpService;
        private readonly EmailVerificationOtpSettings _emailVerificationOtpSettings;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly RefreshTokenSettings _refreshTokenSettings;
        private readonly IRedisCacheService _cache;
        private readonly IUsersMapper _mapper;

        public VerifyEmailOtpCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<VerifyEmailOtpCommandHandler> logger,
            IUserRepository userRepository,
            IEmailVerificationOtpRepository emailVerificationOtpRepository,
            IEmailVerificationOtpService emailVerificationOtpService,
            IOptions<EmailVerificationOtpSettings> emailVerificationOtpOptions,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<RefreshTokenSettings> refreshTokenOptions,
            IRedisCacheService cache,
            IUsersMapper mapper)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailVerificationOtpRepository = emailVerificationOtpRepository ?? throw new ArgumentNullException(nameof(emailVerificationOtpRepository));
            _emailVerificationOtpService = emailVerificationOtpService ?? throw new ArgumentNullException(nameof(emailVerificationOtpService));
            _emailVerificationOtpSettings = emailVerificationOtpOptions?.Value ?? throw new ArgumentNullException(nameof(emailVerificationOtpOptions));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _refreshTokenSettings = refreshTokenOptions?.Value ?? throw new ArgumentNullException(nameof(refreshTokenOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<AuthTokenResponseDTO>>> HandleRequest(
            VerifyEmailOtpCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is null)
            {
                return Error.Unauthorized("Auth.EmailOtp.Invalid", "Invalid verification code.");
            }

            if (user.IsEmailVerified)
            {
                return Error.Conflict("Auth.EmailOtp.AlreadyVerified", "Email is already verified.");
            }

            var otp = await _emailVerificationOtpRepository.GetLatestByUserIdAndPurposeAsync(
                user.Id,
                OtpPurpose.Registration,
                cancellationToken);

            if (otp is null || otp.IsUsed)
            {
                return Error.Validation("Auth.EmailOtp.Missing", "Verification code is no longer valid. Request a new code.");
            }

            if (otp.IsExpired)
            {
                otp.Invalidate();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Error.Validation("Auth.EmailOtp.Expired", "Verification code expired. Request a new code.");
            }

            var codeIsValid = _emailVerificationOtpService.VerifyCode(request.Otp, otp.CodeSalt, otp.CodeHash);
            if (!codeIsValid)
            {
                var blocked = otp.RegisterFailedAttempt(_emailVerificationOtpSettings.MaxVerificationAttempts);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (blocked)
                {
                    return Error.Forbidden(
                        "Auth.EmailOtp.AttemptsExceeded",
                        "Too many invalid attempts. Request a new code.");
                }

                return Error.Unauthorized("Auth.EmailOtp.Invalid", "Invalid verification code.");
            }

            otp.Consume();
            user.MarkEmailAsVerified();
            user.Activate();
            user.RaiseUserRegistered();

            var isFirstLogin = user.RecordLogin();
            var token = _jwtTokenService.CreateToken(user);
            var refreshDescriptor = _refreshTokenService.CreateToken();
            var refreshToken = RefreshTokenEntity.Create(
                userId: user.Id,
                tokenHash: refreshDescriptor.TokenHash,
                expiresAt: refreshDescriptor.ExpiresAt,
                createdByIp: request.RequestIp,
                createdByUserAgent: request.UserAgent);

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _refreshTokenRepository.RevokeOldestActiveTokensAsync(
                user.Id,
                _refreshTokenSettings.MaxActiveTokensPerUser,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await MsAuthApiCache.InvalidateUserAsync(_cache, user.Id, user.AuthUserId, _logger);

            await RefreshTokenCache.TrySetAsync(
                _cache,
                refreshDescriptor.TokenHash,
                new RefreshTokenCacheEntry(refreshToken.Id, refreshToken.UserId, refreshToken.ExpiresAt, refreshToken.RevokedAt),
                refreshDescriptor.ExpiresAt,
                _logger);

            var userSession = _mapper.ToUserSessionResponse(user);
            userSession.IsFirstLogin = isFirstLogin;

            var response = new AuthTokenResponseDTO
            {
                AccessToken = token.AccessToken,
                ExpiresAt = token.ExpiresAt,
                RefreshToken = refreshDescriptor.Token,
                RefreshTokenExpiresAt = refreshDescriptor.ExpiresAt,
                IsFirstLogin = isFirstLogin,
                User = userSession
            };

            return new ApiResponse<AuthTokenResponseDTO>(response, true);
        }
    }
}

