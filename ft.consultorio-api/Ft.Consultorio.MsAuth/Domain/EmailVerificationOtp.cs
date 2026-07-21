using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsAuth.Domain
{
    public sealed class EmailVerificationOtp : BaseEntity
    {
        public Guid UserId { get; private set; }

        /// <summary>
        /// Correo asociado al OTP. En <see cref="OtpPurpose.Registration"/> es el correo
        /// actual del usuario; en <see cref="OtpPurpose.EmailChange"/> es el correo NUEVO
        /// pendiente de confirmar.
        /// </summary>
        public string Email { get; private set; } = default!;
        public OtpPurpose Purpose { get; private set; }
        public string CodeHash { get; private set; } = default!;
        public string CodeSalt { get; private set; } = default!;
        public DateTimeOffset ExpiresAt { get; private set; }
        public int Attempts { get; private set; }
        public DateTimeOffset? ConsumedAt { get; private set; }

        public User User { get; private set; } = default!;

        public bool IsUsed => ConsumedAt is not null;
        public bool IsExpired => ExpiresAt <= DateTimeOffset.UtcNow;
        public bool IsAvailable => !IsUsed && !IsExpired;

        private EmailVerificationOtp() { }

        private EmailVerificationOtp(
            Guid userId,
            string email,
            OtpPurpose purpose,
            string codeHash,
            string codeSalt,
            DateTimeOffset expiresAt,
            Guid? id = null) : base(userId, id)
        {
            UserId = userId;
            Email = email;
            Purpose = purpose;
            CodeHash = codeHash;
            CodeSalt = codeSalt;
            ExpiresAt = expiresAt;
            Attempts = 0;
            ConsumedAt = null;
        }

        public static EmailVerificationOtp Create(
            Guid userId,
            string email,
            string codeHash,
            string codeSalt,
            DateTimeOffset expiresAt,
            OtpPurpose purpose = OtpPurpose.Registration,
            Guid? id = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (string.IsNullOrWhiteSpace(codeHash))
                throw new ArgumentException("CodeHash is required.", nameof(codeHash));

            if (string.IsNullOrWhiteSpace(codeSalt))
                throw new ArgumentException("CodeSalt is required.", nameof(codeSalt));

            if (expiresAt <= DateTimeOffset.UtcNow)
                throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

            return new EmailVerificationOtp(
                userId,
                email.Trim().ToLowerInvariant(),
                purpose,
                codeHash.Trim(),
                codeSalt.Trim(),
                expiresAt,
                id);
        }

        public void ReplaceCode(
            string codeHash,
            string codeSalt,
            DateTimeOffset expiresAt)
        {
            if (string.IsNullOrWhiteSpace(codeHash))
                throw new ArgumentException("CodeHash is required.", nameof(codeHash));

            if (string.IsNullOrWhiteSpace(codeSalt))
                throw new ArgumentException("CodeSalt is required.", nameof(codeSalt));

            if (expiresAt <= DateTimeOffset.UtcNow)
                throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

            CodeHash = codeHash.Trim();
            CodeSalt = codeSalt.Trim();
            ExpiresAt = expiresAt;
            Attempts = 0;
            ConsumedAt = null;
            SetAuditUpdate(UserId);
        }

        public bool RegisterFailedAttempt(int maxAttempts)
        {
            if (maxAttempts <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be greater than zero.");

            if (ConsumedAt is not null)
            {
                return true;
            }

            Attempts++;
            SetAuditUpdate(UserId);

            if (Attempts >= maxAttempts)
            {
                ConsumedAt = DateTimeOffset.UtcNow;
                return true;
            }

            return false;
        }

        public void Consume()
        {
            if (ConsumedAt is not null) return;

            ConsumedAt = DateTimeOffset.UtcNow;
            SetAuditUpdate(UserId);
        }

        public void Invalidate()
        {
            if (ConsumedAt is not null) return;

            ConsumedAt = DateTimeOffset.UtcNow;
            SetAuditUpdate(UserId);
        }
    }
}
