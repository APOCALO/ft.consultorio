namespace Ft.Consultorio.MsAuth.Infrastructure.Settings
{
    /// <summary>
    /// Settings for the email verification OTP service.
    /// </summary>
    /// <remarks>
    /// The hash algorithm defaults to Argon2id (recommended by OWASP 2024 and NIST SP 800-63B)
    /// and the <see cref="Pepper"/> is a server-side secret (env var, Key Vault) that augments
    /// the per-code salt. Both are required at construction time and validated for strength.
    /// </remarks>
    public sealed record EmailVerificationOtpSettings
    {
        /// <summary>
        /// Cuando es <c>false</c>, el registro activa y verifica la cuenta al instante
        /// (sin OTP ni envío de correo) y el usuario puede iniciar sesión de inmediato.
        /// Útil mientras no exista un microservicio de notificaciones para enviar correos.
        /// Por defecto <c>true</c> (se exige verificación por OTP).
        /// </summary>
        public bool Enabled { get; init; } = true;

        public int TtlMinutes { get; init; } = 10;

        /// <summary>
        /// Length of the numeric OTP code (number of digits).
        /// Validated in [6, 8] to ensure sufficient entropy for a memory-hard KDF.
        /// </summary>
        public int CodeLength { get; init; } = 6;

        public int ResendCooldownSeconds { get; init; } = 60;
        public int MaxSendsPerHourPerEmail { get; init; } = 5;
        public int MaxVerificationAttempts { get; init; } = 5;

        /// <summary>
        /// Server-side secret (base64, at least 32 bytes when decoded) that augments the
        /// per-code salt. Stored OUTSIDE the database (env var, Key Vault, etc.).
        /// Rotating the pepper invalidates ALL existing OTP hashes.
        /// </summary>
        public string Pepper { get; init; } = string.Empty;

        /// <summary>
        /// Argon2id memory cost in KiB. Default 64 MiB is OWASP's recommended baseline.
        /// Increase for higher security at the cost of CPU per verification.
        /// </summary>
        public int Argon2MemoryKiB { get; init; } = 64 * 1024;

        /// <summary>
        /// Argon2id iteration count. Default 3 matches OWASP's recommended baseline.
        /// </summary>
        public int Argon2Iterations { get; init; } = 3;

        /// <summary>
        /// Argon2id degree of parallelism. Default 1 is the safest cross-platform setting.
        /// </summary>
        public int Argon2Parallelism { get; init; } = 1;

        /// <summary>
        /// Length of the Argon2id output hash in bytes. Default 32 matches the SHA-256
        /// output length used by the legacy v1 hash format, simplifying migration.
        /// </summary>
        public int HashByteLength { get; init; } = 32;
    }
}
