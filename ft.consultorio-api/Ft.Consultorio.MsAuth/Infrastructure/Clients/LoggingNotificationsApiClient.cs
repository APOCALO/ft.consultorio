using Ft.Consultorio.MsAuth.Application.Interfaces.Clients;

namespace Ft.Consultorio.MsAuth.Infrastructure.Clients
{
    /// <summary>
    /// Adaptador temporal del puerto <see cref="INotificationsApiClient"/>. Mientras el
    /// microservicio de notificaciones no forma parte de esta solución, registra en el log
    /// el contenido que se enviaría por correo (OTP, enlace de restablecimiento, bienvenida)
    /// para no bloquear los flujos de auth en desarrollo.
    ///
    /// Cuando se agregue MsNotifications, sustituir el registro de esta implementación por
    /// un cliente HTTP real en <c>DependencyInjection.AddNotificationsClient</c>.
    /// </summary>
    internal sealed class LoggingNotificationsApiClient : INotificationsApiClient
    {
        private readonly ILogger<LoggingNotificationsApiClient> _logger;

        public LoggingNotificationsApiClient(ILogger<LoggingNotificationsApiClient> logger)
        {
            _logger = logger;
        }

        public Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[Notifications:STUB] Correo de bienvenida para {Email} ({FirstName}).",
                to, firstName);
            return Task.CompletedTask;
        }

        public Task SendEmailVerificationOtpAsync(
            string to,
            string firstName,
            string otpCode,
            int expiresInMinutes,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[Notifications:STUB] OTP de verificación para {Email} ({FirstName}): {OtpCode} (expira en {Minutes} min).",
                to, firstName, otpCode, expiresInMinutes);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(
            string to,
            string firstName,
            string resetLink,
            int expiresInMinutes,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[Notifications:STUB] Enlace de restablecimiento para {Email} ({FirstName}): {ResetLink} (expira en {Minutes} min).",
                to, firstName, resetLink, expiresInMinutes);
            return Task.CompletedTask;
        }
    }
}
