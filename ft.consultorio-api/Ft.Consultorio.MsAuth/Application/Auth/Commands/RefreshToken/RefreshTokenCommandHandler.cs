using Ft.Consultorio.MsAuth.Application.Mapping;
using ErrorOr;
using Microsoft.Extensions.Options;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Users.DTOs;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using RefreshTokenEntity = Ft.Consultorio.MsAuth.Domain.RefreshToken;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.MsAuth.Application.Auth;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RefreshToken
{
    internal sealed class RefreshTokenCommandHandler
        : ApiBaseHandler<RefreshTokenCommand, AuthTokenResponseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly RefreshTokenSettings _refreshTokenSettings;
        private readonly IRedisCacheService _cache;
        private readonly IDistributedLock _lock;
        private readonly IUsersMapper _mapper;

        public RefreshTokenCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<RefreshTokenCommandHandler> logger,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IOptions<RefreshTokenSettings> refreshTokenOptions,
            IRedisCacheService cache,
            IDistributedLock @lock,
            IUsersMapper mapper)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _refreshTokenSettings = refreshTokenOptions?.Value ?? throw new ArgumentNullException(nameof(refreshTokenOptions));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override async Task<ErrorOr<ApiResponse<AuthTokenResponseDTO>>> HandleRequest(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var rawToken = request.RefreshToken.Trim();
            var tokenHash = _refreshTokenService.HashToken(rawToken);
            var cachedEntry = await RefreshTokenCache.TryGetAsync(_cache, tokenHash, _logger);

            if (cachedEntry is not null)
            {
                if (cachedEntry.RevokedAt is not null)
                {
                    await _refreshTokenRepository.RevokeAllActiveForUserAsync(
                        cachedEntry.UserId,
                        "Refresh token reuse detected.",
                        cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Error.Unauthorized("Auth.Refresh.Reused", "Refresh token is no longer valid.");
                }

                if (cachedEntry.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    return Error.Unauthorized("Auth.Refresh.Expired", "Refresh token expired.");
                }
            }

            var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            if (refreshToken is null)
            {
                return Error.Unauthorized("Auth.Refresh.Invalid", "Invalid refresh token.");
            }

            // --- Distributed lock: serialize refresh operations per user ---
            // Prevents race condition where 2+ tabs refresh the same token
            // simultaneously, both pass validation, and both generate new tokens
            // (leaving one tab with an orphaned token on next refresh).
            var lockKey = $"auth:refresh-lock:{refreshToken.UserId}";
            await using var lockHandle = await _lock.AcquireAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);
            if (lockHandle is null)
            {
                return Error.Conflict("Auth.Refresh.Busy", "Another refresh is in progress. Please retry.");
            }

            // Re-fetch token state inside the lock — another request may have
            // already rotated (revoked) this token while we were waiting.
            refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
            if (refreshToken is null)
            {
                return Error.Unauthorized("Auth.Refresh.Invalid", "Invalid refresh token.");
            }

            if (refreshToken.RevokedAt is not null)
            {
                await _refreshTokenRepository.RevokeAllActiveForUserAsync(
                    refreshToken.UserId,
                    "Refresh token reuse detected.",
                    cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await RefreshTokenCache.TrySetAsync(
                    _cache,
                    refreshToken.TokenHash,
                    new RefreshTokenCacheEntry(refreshToken.Id, refreshToken.UserId, refreshToken.ExpiresAt, refreshToken.RevokedAt),
                    refreshToken.ExpiresAt,
                    _logger);
                return Error.Unauthorized("Auth.Refresh.Reused", "Refresh token is no longer valid.");
            }

            if (refreshToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                refreshToken.Revoke("Refresh token expired.", revokedByIp: null, replacedByTokenHash: null);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await RefreshTokenCache.TrySetAsync(
                    _cache,
                    refreshToken.TokenHash,
                    new RefreshTokenCacheEntry(refreshToken.Id, refreshToken.UserId, refreshToken.ExpiresAt, refreshToken.RevokedAt),
                    refreshToken.ExpiresAt,
                    _logger);
                return Error.Unauthorized("Auth.Refresh.Expired", "Refresh token expired.");
            }

            var user = await _userRepository.GetByIdWithRolesAsync(refreshToken.UserId, cancellationToken);
            if (user is null)
            {
                return Error.Unauthorized("Auth.Refresh.InvalidUser", "Invalid refresh token.");
            }

            if (!user.IsActive)
            {
                return Error.Forbidden("Auth.Refresh.Inactive", "User is inactive.");
            }

            var accessToken = _jwtTokenService.CreateToken(user);
            var newRefreshDescriptor = _refreshTokenService.CreateToken();
            var newRefreshToken = RefreshTokenEntity.Create(
                userId: user.Id,
                tokenHash: newRefreshDescriptor.TokenHash,
                expiresAt: newRefreshDescriptor.ExpiresAt,
                createdByIp: null,
                createdByUserAgent: null);

            refreshToken.Revoke("Rotated refresh token.", revokedByIp: null, replacedByTokenHash: newRefreshDescriptor.TokenHash);

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
            await _refreshTokenRepository.RevokeOldestActiveTokensAsync(
                user.Id,
                _refreshTokenSettings.MaxActiveTokensPerUser,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await RefreshTokenCache.TrySetAsync(
                _cache,
                refreshToken.TokenHash,
                new RefreshTokenCacheEntry(refreshToken.Id, refreshToken.UserId, refreshToken.ExpiresAt, refreshToken.RevokedAt),
                refreshToken.ExpiresAt,
                _logger);
            await RefreshTokenCache.TrySetAsync(
                _cache,
                newRefreshDescriptor.TokenHash,
                new RefreshTokenCacheEntry(newRefreshToken.Id, newRefreshToken.UserId, newRefreshToken.ExpiresAt, newRefreshToken.RevokedAt),
                newRefreshDescriptor.ExpiresAt,
                _logger);

            var response = new AuthTokenResponseDTO
            {
                AccessToken = accessToken.AccessToken,
                ExpiresAt = accessToken.ExpiresAt,
                RefreshToken = newRefreshDescriptor.Token,
                RefreshTokenExpiresAt = newRefreshDescriptor.ExpiresAt,
                User = _mapper.ToUserSessionResponse(user)
            };

            return new ApiResponse<AuthTokenResponseDTO>(response, true);
        }
    }
}


