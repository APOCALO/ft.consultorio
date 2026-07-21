namespace Ft.Consultorio.MsAuth.Infrastructure.Settings
{
    /// <summary>
    /// Configuración del captcha de Cloudflare Turnstile usado en los endpoints
    /// públicos de autenticación (login y registro).
    /// </summary>
    public sealed class TurnstileSettings
    {
        /// <summary>
        /// Activa/desactiva la verificación. Si es <c>false</c> el token no se valida
        /// (degradación elegante en entornos sin claves). Por defecto activado.
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>Secret key de Turnstile. NUNCA exponer al cliente.</summary>
        public string SecretKey { get; init; } = string.Empty;

        /// <summary>Endpoint de verificación server-side de Cloudflare.</summary>
        public string VerifyEndpoint { get; init; } =
            "https://challenges.cloudflare.com/turnstile/v0/siteverify";
    }
}
