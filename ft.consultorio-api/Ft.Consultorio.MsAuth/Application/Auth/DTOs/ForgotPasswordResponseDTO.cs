namespace Ft.Consultorio.MsAuth.Application.Auth.DTOs
{
    public sealed class ForgotPasswordResponseDTO
    {
        public string Message { get; init; } = "Si existe una cuenta, te enviaremos un enlace para restablecer tu contraseña.";
    }
}
