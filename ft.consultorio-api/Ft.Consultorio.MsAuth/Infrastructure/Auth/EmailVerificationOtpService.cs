using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using Ft.Consultorio.MsAuth.Application.Interfaces.Auth;
using Ft.Consultorio.MsAuth.Infrastructure.Settings;

namespace Ft.Consultorio.MsAuth.Infrastructure.Auth
{
    /// <summary>
    /// Email verification OTP service using Argon2id (OWASP 2024 recommended) with a
    /// server-side pepper for defense-in-depth.
    /// </summary>
    /// <remarks>
    /// Hash format is version-prefixed to enable gradual migration from the legacy
    /// SHA-256 single-iteration implementation without invalidating outstanding OTPs:
    /// <list type="bullet">
    ///   <item><c>v1$</c> = legacy SHA-256 (salt || code), kept for verify-only paths</item>
    ///   <item><c>v2$</c> = Argon2id (per-code salt in input, pepper as Argon2 salt)</item>
    /// </list>
    /// New OTPs are always issued as <c>v2$</c>.
    /// </remarks>
    public sealed class EmailVerificationOtpService : IEmailVerificationOtpService
    {
        // Hash version prefixes.
        internal const string LegacyVersionPrefix = "v1$";
        internal const string Argon2VersionPrefix = "v2$";

        // Pepper is at least 32 bytes when decoded.
        private const int PepperMinBytes = 32;

        // The per-code salt is 16 bytes (128 bits) — well above OWASP's 64-bit minimum.
        private const int PerCodeSaltBytes = 16;

        private readonly EmailVerificationOtpSettings _settings;
        private readonly byte[] _pepper;

        public EmailVerificationOtpService(IOptions<EmailVerificationOtpSettings> options)
        {
            _settings = options?.Value
                ?? throw new InvalidOperationException("EmailVerificationOtpSettings are missing.");

            // Fail-fast: surface misconfigurations at startup, not at first request.
            ValidateRange(nameof(_settings.TtlMinutes), _settings.TtlMinutes, 1, 60);
            ValidateRange(nameof(_settings.CodeLength), _settings.CodeLength, 6, 8);
            ValidateRange(nameof(_settings.ResendCooldownSeconds), _settings.ResendCooldownSeconds, 1, 600);
            ValidateRange(nameof(_settings.MaxSendsPerHourPerEmail), _settings.MaxSendsPerHourPerEmail, 1, 20);
            ValidateRange(nameof(_settings.MaxVerificationAttempts), _settings.MaxVerificationAttempts, 3, 10);
            ValidateRange(nameof(_settings.Argon2MemoryKiB), _settings.Argon2MemoryKiB, 8 * 1024, 1024 * 1024);
            ValidateRange(nameof(_settings.Argon2Iterations), _settings.Argon2Iterations, 1, 10);
            ValidateRange(nameof(_settings.Argon2Parallelism), _settings.Argon2Parallelism, 1, 8);
            ValidateRange(nameof(_settings.HashByteLength), _settings.HashByteLength, 16, 64);

            if (string.IsNullOrWhiteSpace(_settings.Pepper))
            {
                throw new InvalidOperationException(
                    "Auth:EmailVerificationOtp:Pepper is required. " +
                    "Generate one with: openssl rand -base64 32");
            }

            byte[] pepperBytes;
            try
            {
                pepperBytes = Convert.FromBase64String(_settings.Pepper);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "Auth:EmailVerificationOtp:Pepper must be a valid base64-encoded string.");
            }

            if (pepperBytes.Length < PepperMinBytes)
            {
                throw new InvalidOperationException(
                    $"Auth:EmailVerificationOtp:Pepper must be at least {PepperMinBytes} bytes when decoded " +
                    $"(got {pepperBytes.Length}). Generate one with: openssl rand -base64 {PepperMinBytes}");
            }

            _pepper = pepperBytes;
        }

        public EmailVerificationOtpDescriptor CreateOtp()
        {
            var maxExclusive = (int)Math.Pow(10, _settings.CodeLength);
            var numericCode = RandomNumberGenerator.GetInt32(0, maxExclusive)
                .ToString($"D{_settings.CodeLength}");

            var perCodeSalt = RandomNumberGenerator.GetBytes(PerCodeSaltBytes);
            var hash = ComputeArgon2Hash(numericCode, perCodeSalt);
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.TtlMinutes);

            return new EmailVerificationOtpDescriptor(
                Code: numericCode,
                CodeHash: hash,
                CodeSalt: Convert.ToBase64String(perCodeSalt),
                ExpiresAt: expiresAt);
        }

        public bool VerifyCode(string code, string codeSalt, string expectedCodeHash)
        {
            if (string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(codeSalt) ||
                string.IsNullOrWhiteSpace(expectedCodeHash))
            {
                return false;
            }

            // Route to the right algorithm based on the version prefix.
            if (expectedCodeHash.StartsWith(Argon2VersionPrefix, StringComparison.Ordinal))
            {
                return VerifyArgon2Hash(code, codeSalt, expectedCodeHash);
            }

            if (expectedCodeHash.StartsWith(LegacyVersionPrefix, StringComparison.Ordinal))
            {
                return VerifyLegacySha256Hash(code, codeSalt, expectedCodeHash);
            }

            // Backward-compat: unprefixed hashes are treated as legacy v1 (original code path).
            // This handles OTP rows created before the migration.
            return VerifyLegacySha256HashRaw(code, codeSalt, expectedCodeHash);
        }

        // --- Argon2id -----------------------------------------------------------

        private string ComputeArgon2Hash(string code, byte[] perCodeSalt)
        {
            // Input layout: perCodeSalt || codeBytes.
            // The pepper is passed as the Argon2id "Salt" parameter — server-side only.
            // An attacker needs both the per-code salt (in DB) and the pepper (env/Key Vault)
            // to brute force.
            var codeBytes = Encoding.UTF8.GetBytes(code.Trim());
            var input = new byte[perCodeSalt.Length + codeBytes.Length];
            Buffer.BlockCopy(perCodeSalt, 0, input, 0, perCodeSalt.Length);
            Buffer.BlockCopy(codeBytes, 0, input, perCodeSalt.Length, codeBytes.Length);

            using var argon2 = new Argon2id(input)
            {
                MemorySize = _settings.Argon2MemoryKiB,
                Iterations = _settings.Argon2Iterations,
                DegreeOfParallelism = _settings.Argon2Parallelism,
                Salt = _pepper,
            };

            var hash = argon2.GetBytes(_settings.HashByteLength);
            return Argon2VersionPrefix + Convert.ToBase64String(hash);
        }

        private bool VerifyArgon2Hash(string code, string codeSalt, string expectedPrefixedHash)
        {
            var storedHashBase64 = expectedPrefixedHash[Argon2VersionPrefix.Length..];
            byte[] perCodeSalt;
            try
            {
                perCodeSalt = Convert.FromBase64String(codeSalt);
            }
            catch (FormatException)
            {
                return false;
            }

            var computedPrefixed = ComputeArgon2Hash(code, perCodeSalt);
            var computed = computedPrefixed[Argon2VersionPrefix.Length..];

            return FixedTimeEqualsBase64(computed, storedHashBase64);
        }

        // --- Legacy SHA-256 (verify-only, supports migration) -------------------

        private static string ComputeLegacySha256Hash(string code, string codeSalt)
        {
            var payload = Encoding.UTF8.GetBytes($"{codeSalt}:{code.Trim()}");
            var hash = SHA256.HashData(payload);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyLegacySha256Hash(string code, string codeSalt, string expectedPrefixedHash)
        {
            var storedHashBase64 = expectedPrefixedHash[LegacyVersionPrefix.Length..];
            var computed = ComputeLegacySha256Hash(code, codeSalt);
            return FixedTimeEqualsBase64(computed, storedHashBase64);
        }

        // Backward-compat path for unprefixed hashes (rows created before migration).
        private static bool VerifyLegacySha256HashRaw(string code, string codeSalt, string expectedHashBase64)
        {
            var computed = ComputeLegacySha256Hash(code, codeSalt);
            return FixedTimeEqualsBase64(computed, expectedHashBase64);
        }

        // --- Helpers ------------------------------------------------------------

        private static bool FixedTimeEqualsBase64(string computed, string stored)
        {
            // We can't use CryptographicOperations.FixedTimeEquals directly on base64 strings
            // because they may have different padding. Decode and compare.
            byte[] left, right;
            try
            {
                left = Convert.FromBase64String(computed);
                right = Convert.FromBase64String(stored);
            }
            catch (FormatException)
            {
                return false;
            }
            return CryptographicOperations.FixedTimeEquals(left, right);
        }

        private static void ValidateRange(string name, int value, int min, int max)
        {
            if (value < min || value > max)
            {
                throw new InvalidOperationException(
                    $"Auth:EmailVerificationOtp:{name} must be in [{min}, {max}]. Got: {value}.");
            }
        }
    }
}
