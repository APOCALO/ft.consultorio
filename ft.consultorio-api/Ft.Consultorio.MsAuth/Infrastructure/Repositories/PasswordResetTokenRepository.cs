using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Data;

namespace Ft.Consultorio.MsAuth.Infrastructure.Repositories
{
    public sealed class PasswordResetTokenRepository
        : BaseRepository<PasswordResetToken, Guid>, IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PasswordResetTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        }

        public async Task InvalidateActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var activeTokens = await _dbContext.PasswordResetTokens
                .Where(token => token.UserId == userId && token.UsedAt == null && token.ExpiresAt > DateTimeOffset.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Invalidate();
            }
        }
    }
}
