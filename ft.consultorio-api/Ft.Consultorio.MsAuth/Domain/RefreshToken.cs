using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Domain
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = default!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }
        public string? ReplacedByTokenHash { get; private set; }
        public string? RevokedReason { get; private set; }
        public string? CreatedByIp { get; private set; }
        public string? CreatedByUserAgent { get; private set; }
        public string? RevokedByIp { get; private set; }

        public User User { get; private set; } = default!;

        public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

        private RefreshToken() { }

        private RefreshToken(
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
            CreatedByIp = string.IsNullOrWhiteSpace(createdByIp) ? null : createdByIp.Trim();
            CreatedByUserAgent = string.IsNullOrWhiteSpace(createdByUserAgent) ? null : createdByUserAgent.Trim();
        }

        public static RefreshToken Create(
            Guid userId,
            string tokenHash,
            DateTimeOffset expiresAt,
            string? createdByIp,
            string? createdByUserAgent,
            Guid? id = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new ArgumentException("TokenHash is required.", nameof(tokenHash));

            if (expiresAt <= DateTimeOffset.UtcNow)
                throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

            return new RefreshToken(
                userId,
                tokenHash.Trim(),
                expiresAt,
                createdByIp,
                createdByUserAgent,
                id);
        }

        public void Revoke(string? reason, string? revokedByIp, string? replacedByTokenHash)
        {
            if (RevokedAt is not null) return;

            RevokedAt = DateTimeOffset.UtcNow;
            RevokedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            RevokedByIp = string.IsNullOrWhiteSpace(revokedByIp) ? null : revokedByIp.Trim();
            ReplacedByTokenHash = string.IsNullOrWhiteSpace(replacedByTokenHash) ? null : replacedByTokenHash.Trim();
        }
    }
}
