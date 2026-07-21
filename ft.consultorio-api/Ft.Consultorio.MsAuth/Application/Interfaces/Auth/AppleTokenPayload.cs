namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    /// <summary>
    /// Datos extraídos del id_token de Apple. Apple NO envía el nombre en el token
    /// (solo la primera vez, en un campo aparte de la respuesta de autorización),
    /// por eso aquí solo viven las claims que sí trae el JWT.
    /// </summary>
    public sealed record AppleTokenPayload(
        string Subject,
        string? Email,
        bool EmailVerified,
        bool IsPrivateEmail);
}
