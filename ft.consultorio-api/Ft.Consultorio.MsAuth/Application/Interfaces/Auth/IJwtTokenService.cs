using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IJwtTokenService
    {
        AuthTokenResult CreateToken(User user);
    }
}
