using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : BaseResponse<AuthTokenResponseDTO>;
}
