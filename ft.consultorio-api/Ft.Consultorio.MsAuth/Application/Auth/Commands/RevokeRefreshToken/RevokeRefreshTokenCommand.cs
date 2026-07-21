using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RevokeRefreshToken
{
    public record RevokeRefreshTokenCommand(string RefreshToken) : BaseResponse<bool>;
}
