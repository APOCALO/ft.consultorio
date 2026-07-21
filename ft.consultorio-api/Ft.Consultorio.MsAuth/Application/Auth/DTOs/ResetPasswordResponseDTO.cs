namespace Ft.Consultorio.MsAuth.Application.Auth.DTOs
{
    public sealed class ResetPasswordResponseDTO
    {
        public bool Success { get; init; } = true;
        public string Message { get; init; } = "Tu contraseña fue actualizada correctamente.";
    }
}
