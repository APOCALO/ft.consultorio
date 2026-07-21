using Ft.Consultorio.MsAuth.Application.Mapping;
using ErrorOr;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Caching;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using RefreshTokenEntity = Ft.Consultorio.MsAuth.Domain.RefreshToken;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeAppleToken
{
    internal sealed class ExchangeAppleTokenCommandHandler
        : ApiBaseHandler<ExchangeAppleTokenCommand, AuthTokenResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IAppleIdTokenValidator _appleIdTokenValidator;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly RefreshTokenSettings _refreshTokenSettings;
        private readonly IRedisCacheService _cache;
        private readonly IUsersMapper _mapper;

        public ExchangeAppleTokenCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ExchangeAppleTokenCommandHandler> logger,
            IUserRepository userRepository,
            IAppleIdTokenValidator appleIdTokenValidator,
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
            _appleIdTokenValidator = appleIdTokenValidator ?? throw new ArgumentNullException(nameof(appleIdTokenValidator));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _refreshTokenSettings = refreshTokenOptions?.Value ?? throw new ArgumentNullException(nameof(refreshTokenOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<AuthTokenResponseDTO>>> HandleRequest(
            ExchangeAppleTokenCommand request,
            CancellationToken cancellationToken)
        {
            AppleTokenPayload payload;
            try
            {
                payload = await _appleIdTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Apple id_token validation failed.");
                return Error.Unauthorized("Auth.Apple.InvalidToken", "Apple token is invalid or expired.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Apple auth configuration error.");
                return Error.Unexpected(description: "Authentication service is misconfigured.");
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                return Error.Unauthorized("Auth.Apple.EmailMissing", "Apple token does not include an email.");
            }

            if (!payload.EmailVerified)
            {
                return Error.Unauthorized("Auth.Apple.EmailUnverified", "Apple email is not verified.");
            }

            var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
            var authUserId = $"apple:{payload.Subject}";

            var user = await _userRepository.GetByAuthUserIdAsync(authUserId, cancellationToken);

            if (user is null)
            {
                var existingByEmail = await _userRepository.GetByEmailAsync(
                    normalizedEmail,
                    asNoTracking: true,
                    cancellationToken);
                if (existingByEmail is not null)
                {
                    return Error.Conflict("Auth.Apple.Email", "Email is already registered with another provider.");
                }

                var userId = Guid.NewGuid();
                var resolvedFirstName = !string.IsNullOrWhiteSpace(request.FirstName)
                    ? request.FirstName!.Trim()
                    : "User";
                var resolvedLastName = !string.IsNullOrWhiteSpace(request.LastName)
                    ? request.LastName!.Trim()
                    : "User";
                var userName = normalizedEmail.Split('@')[0];
                if (string.IsNullOrWhiteSpace(userName))
                {
                    userName = $"user{userId.ToString("N")[..8]}";
                }

                user = User.Create(
                    createdById: userId,
                    authUserId: authUserId,
                    userName: userName,
                    userEmail: normalizedEmail,
                    firstName: resolvedFirstName,
                    lastName: resolvedLastName,
                    birthDate: null,
                    avatarUrl: null,
                    isActive: true,
                    isEmailVerified: true,
                    settings: UserSettings.Create(userId, null, null, null),
                    id: userId);

                user.RaiseUserRegistered();
                await _userRepository.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (!user.IsActive)
                {
                    return Error.Forbidden("Auth.Apple.Inactive", "User is inactive.");
                }

                if (!user.IsEmailVerified)
                {
                    user.MarkEmailAsVerified();
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            var isFirstLogin = user.RecordLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var token = _jwtTokenService.CreateToken(user);
            var refreshDescriptor = _refreshTokenService.CreateToken();
            var refreshToken = RefreshTokenEntity.Create(
                userId: user.Id,
                tokenHash: refreshDescriptor.TokenHash,
                expiresAt: refreshDescriptor.ExpiresAt,
                createdByIp: null,
                createdByUserAgent: null);

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
