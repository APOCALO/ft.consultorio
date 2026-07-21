using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.MsAuth.Application.Auth;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RevokeRefreshToken
{
    internal sealed class RevokeRefreshTokenCommandHandler
        : ApiBaseHandler<RevokeRefreshTokenCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRedisCacheService _cache;

        public RevokeRefreshTokenCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<RevokeRefreshTokenCommandHandler> logger,
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            IRedisCacheService cache)
            : base(logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override async Task<ErrorOr<ApiResponse<bool>>> HandleRequest(
            RevokeRefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var tokenHash = _refreshTokenService.HashToken(request.RefreshToken.Trim());
            var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            if (refreshToken is not null && refreshToken.RevokedAt is null)
            {
                refreshToken.Revoke("Logged out.", revokedByIp: null, replacedByTokenHash: null);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await RefreshTokenCache.TrySetAsync(
                    _cache,
                    refreshToken.TokenHash,
                    new RefreshTokenCacheEntry(refreshToken.Id, refreshToken.UserId, refreshToken.ExpiresAt, refreshToken.RevokedAt),
                    refreshToken.ExpiresAt,
                    _logger);
            }

            return new ApiResponse<bool>(true, true);
        }
    }
}
