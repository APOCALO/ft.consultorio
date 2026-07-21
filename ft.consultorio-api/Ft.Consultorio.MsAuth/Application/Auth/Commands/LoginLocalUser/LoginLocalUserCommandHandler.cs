using Ft.Consultorio.MsAuth.Application.Mapping;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsAuth.Application.Auth;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Application.Users.DTOs;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using RefreshTokenEntity = Ft.Consultorio.MsAuth.Domain.RefreshToken;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.LoginLocalUser
{
    internal sealed class LoginLocalUserCommandHandler
        : ApiBaseHandler<LoginLocalUserCommand, AuthTokenResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly RefreshTokenSettings _refreshTokenSettings;
        private readonly IRedisCacheService _cache;
        private readonly IUsersMapper _mapper;
        private readonly ITurnstileVerifier _turnstileVerifier;

        public LoginLocalUserCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<LoginLocalUserCommandHandler> logger,
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<RefreshTokenSettings> refreshTokenOptions,
            IRedisCacheService cache,
            IUsersMapper mapper,
            ITurnstileVerifier turnstileVerifier)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _refreshTokenSettings = refreshTokenOptions?.Value ?? throw new ArgumentNullException(nameof(refreshTokenOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _turnstileVerifier = turnstileVerifier ?? throw new ArgumentNullException(nameof(turnstileVerifier));
        }

        protected override async Task<ErrorOr<ApiResponse<AuthTokenResponseDTO>>> HandleRequest(
            LoginLocalUserCommand request,
            CancellationToken cancellationToken)
        {
            // Verificación del captcha antes de tocar la base de datos o el cache.
            var captchaValid = await _turnstileVerifier.VerifyAsync(
                request.CaptchaToken,
                request.RequestIp,
                cancellationToken);

            if (!captchaValid)
            {
                return Error.Forbidden("Auth.Login.CaptchaInvalid", "Captcha verification failed.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var attemptKey = AuthAttemptLimiter.BuildKey("login", normalizedEmail, request.RequestIp);
            var lockoutError = await AuthAttemptLimiter.CheckLockoutAsync(
                _cache,
                attemptKey,
                _logger,
                "Auth.Login.Throttled");

            if (lockoutError is not null)
            {
                return lockoutError.Value;
            }

            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return Error.Unauthorized("Auth.Login.Invalid", "Invalid credentials.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return Error.Unauthorized("Auth.Login.Invalid", "Invalid credentials.");
            }

            if (!user.IsEmailVerified)
            {
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return Error.Forbidden("Auth.Login.EmailNotVerified", "Email is not verified.");
            }

            if (!user.IsActive)
            {
                await AuthAttemptLimiter.RecordFailureAsync(_cache, attemptKey, _logger);
                return Error.Forbidden("Auth.Login.Inactive", "User is inactive.");
            }

            await AuthAttemptLimiter.ClearAsync(_cache, attemptKey, _logger);

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var isFirstLogin = user.RecordLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

