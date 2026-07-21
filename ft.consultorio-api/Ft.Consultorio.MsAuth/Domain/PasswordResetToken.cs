using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Domain
{
    public sealed class PasswordResetToken : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = default!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? UsedAt { get; private set; }
        public int Attempts { get; private set; }
        public string? CreatedByIp { get; private set; }
        public string? CreatedByUserAgent { get; private set; }

        public User User { get; private set; } = default!;

        public bool IsUsed => UsedAt is not null;
        public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
        public bool IsActive => !IsUsed && !IsExpired;

        private PasswordResetToken() { }

        private PasswordResetToken(
            Guid userId,
            string tokenHash,
            DateTimeOffset expiresAt,
            string? createdByIp,
            string? createdByUserAgent,
            Guid? id = null) : base(userId, id)
        {
            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
            UsedAt = null;
            Attempts = 0;
            CreatedByIp = string.IsNullOrWhiteSpace(createdByIp) ? null : createdByIp.Trim();
            CreatedByUserAgent = string.IsNullOrWhiteSpace(createdByUserAgent) ? null : createdByUserAgent.Trim();
        }

        public static PasswordResetToken Create(
            Guid userId,
            string tokenHash,
            DateTimeOffset expiresAt,
            string? createdByIp,
            string? createdByUserAgent,
            Guid? id = null)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new ArgumentException("TokenHash is required.", nameof(tokenHash));
            }

            if (expiresAt <= DateTimeOffset.UtcNow)
            {
                throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));
            }

            return new PasswordResetToken(
                userId,
                tokenHash.Trim(),
                expiresAt,
                createdByIp,
                createdByUserAgent,
                id);
        }

        public bool RegisterFailedAttempt(int maxAttempts)
        {
            if (maxAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be greater than zero.");
            }

            if (IsUsed)
            {
                return true;
            }

            Attempts++;
            SetAuditUpdate(UserId);

            if (Attempts >= maxAttempts)
            {
                UsedAt = DateTimeOffset.UtcNow;
                return true;
            }

            return false;
        }

        public void MarkAsUsed()
        {
            if (UsedAt is not null) return;

            UsedAt = DateTimeOffset.UtcNow;
            SetAuditUpdate(UserId);
        }

        public void Invalidate()
        {
            if (UsedAt is not null) return;

            UsedAt = DateTimeOffset.UtcNow;
            SetAuditUpdate(UserId);
        }
    }
}
