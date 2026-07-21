namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    /// <summary>
    /// Valida un token de Cloudflare Turnstile contra el endpoint de verificación
    /// server-side. Implementación "fail closed": ante token vacío, fallo de red o
    /// respuesta negativa, devuelve <c>false</c>.
    /// </summary>
    public interface ITurnstileVerifier
    {
        /// <summary>
        /// Verifica el token del captcha. Si la verificación está deshabilitada por
        /// configuración, devuelve <c>true</c> sin contactar a Cloudflare.
        /// </summary>
        /// <param name="token">Token emitido por el widget en el cliente.</param>
        /// <param name="remoteIp">IP del cliente (opcional, mejora la señal anti-fraude).</param>
        Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken);
    }
}
