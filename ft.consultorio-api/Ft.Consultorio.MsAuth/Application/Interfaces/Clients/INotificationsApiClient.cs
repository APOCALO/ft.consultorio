namespace Ft.Consultorio.MsAuth.Application.Interfaces.Clients
{
    public interface INotificationsApiClient
    {
        Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken ct = default);
        Task SendEmailVerificationOtpAsync(
            string to,
            string firstName,
            string otpCode,
            int expiresInMinutes,
            CancellationToken ct = default);
        Task SendPasswordResetLinkAsync(
            string to,
            string firstName,
            string resetLink,
            int expiresInMinutes,
            CancellationToken ct = default);
    }
}
