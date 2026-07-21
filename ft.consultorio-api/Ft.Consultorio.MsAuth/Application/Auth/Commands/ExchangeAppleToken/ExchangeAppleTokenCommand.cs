using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeAppleToken
{
    /// <summary>
    /// Intercambia el id_token de Apple por la sesión propia. FirstName/LastName solo
    /// llegan la primera vez que el usuario autoriza (Apple no los repite después);
    /// se usan únicamente al crear el usuario.
    /// </summary>
    public record ExchangeAppleTokenCommand(
        string IdToken,
        string? FirstName = null,
        string? LastName = null) : BaseResponse<AuthTokenResponseDTO>;
}
