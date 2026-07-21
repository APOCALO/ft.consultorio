using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeGoogleToken
{
    public record ExchangeGoogleTokenCommand(string IdToken, string? GoogleAccessToken = null) : BaseResponse<AuthTokenResponseDTO>;
}
