using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;
using Ft.Consultorio.MsAuth.Application.Interfaces.Repositories;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.MsAuth.Infrastructure.Data;

namespace Ft.Consultorio.MsAuth.Infrastructure.Repositories
{
    public sealed class EmailVerificationOtpRepository
        : BaseRepository<EmailVerificationOtp, Guid>, IEmailVerificationOtpRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public EmailVerificationOtpRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<EmailVerificationOtp?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return GetLatestByUserIdAsync(userId, asNoTracking: false, cancellationToken);
        }

        public async Task<EmailVerificationOtp?> GetLatestByUserIdAsync(
            Guid userId,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.EmailVerificationOtps
                .Where(otp => otp.UserId == userId)
                .OrderByDescending(otp => otp.CreatedAt)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<EmailVerificationOtp?> GetLatestByUserIdAndPurposeAsync(
            Guid userId,
            OtpPurpose purpose,
            CancellationToken cancellationToken)
        {
            return await _dbContext.EmailVerificationOtps
                .Where(otp => otp.UserId == userId && otp.Purpose == purpose)
                .OrderByDescending(otp => otp.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<EmailVerificationOtp?> GetLatestByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return GetLatestByEmailAsync(email, asNoTracking: false, cancellationToken);
        }

        public async Task<EmailVerificationOtp?> GetLatestByEmailAsync(
            string email,
            bool asNoTracking,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var query = _dbContext.EmailVerificationOtps
                .Where(otp => otp.Email == normalizedEmail)
                .OrderByDescending(otp => otp.CreatedAt)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InvalidateActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var activeOtps = await _dbContext.EmailVerificationOtps
                .Where(otp => otp.UserId == userId && otp.ConsumedAt == null && otp.ExpiresAt > DateTimeOffset.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var otp in activeOtps)
            {
                otp.Invalidate();
            }
        }

        public async Task InvalidateActiveByUserIdAndPurposeAsync(
            Guid userId,
            OtpPurpose purpose,
            CancellationToken cancellationToken)
        {
            var activeOtps = await _dbContext.EmailVerificationOtps
                .Where(otp => otp.UserId == userId
                    && otp.Purpose == purpose
                    && otp.ConsumedAt == null
                    && otp.ExpiresAt > DateTimeOffset.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var otp in activeOtps)
            {
                otp.Invalidate();
            }
        }
    }
}
