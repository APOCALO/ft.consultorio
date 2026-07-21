namespace Ft.Consultorio.ServiceDefaults.Infrastructure.Settings
{
    public sealed class JwtSettings
    {
        /// <summary>
        /// Clave pública RSA (PEM) para validar tokens RS256. Es la vía recomendada:
        /// solo el emisor (MsAuth) posee la privada, los demás servicios únicamente validan.
        /// </summary>
        public string PublicKeyPem { get; init; } = string.Empty;

        /// <summary>
        /// Clave privada RSA (PEM) para firmar tokens. Solo debe configurarse en el emisor.
        /// </summary>
        public string PrivateKeyPem { get; init; } = string.Empty;

        /// <summary>
        /// Clave simétrica HS256 (Base64). Camino legado: solo se usa si no hay
        /// PublicKeyPem configurada (p. ej. hosts de pruebas de integración).
        /// </summary>
        public string SecretKey { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string[] AdditionalIssuers { get; init; } = [];
        public string[] AdditionalAudiences { get; init; } = [];
        public int ExpiryMinutes { get; init; } = 60;
    }
}
