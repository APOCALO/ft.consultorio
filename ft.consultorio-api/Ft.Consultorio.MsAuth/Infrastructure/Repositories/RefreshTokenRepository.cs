using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Data;

namespace Ft.Consultorio.MsAuth.Infrastructure.Repositories
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken, Guid>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RefreshTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            return await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
                .OrderBy(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task RevokeAllActiveForUserAsync(Guid userId, string reason, CancellationToken cancellationToken)
        {
            var tokens = await GetActiveTokensByUserAsync(userId, cancellationToken);
            foreach (var token in tokens)
            {
                token.Revoke(reason, revokedByIp: null, replacedByTokenHash: null);
            }
        }

        public async Task RevokeOldestActiveTokensAsync(Guid userId, int maxActiveTokens, CancellationToken cancellationToken)
        {
            if (maxActiveTokens <= 0) return;

            var tokens = await GetActiveTokensByUserAsync(userId, cancellationToken);
            var overflow = tokens.Count - maxActiveTokens;
            if (overflow <= 0) return;

            foreach (var token in tokens.Take(overflow))
            {
                token.Revoke("Exceeded max active refresh tokens.", revokedByIp: null, replacedByTokenHash: null);
            }
        }
    }
}
