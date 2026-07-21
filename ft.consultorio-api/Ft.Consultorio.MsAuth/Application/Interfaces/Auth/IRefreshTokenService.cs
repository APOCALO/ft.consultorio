namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IRefreshTokenService
    {
        RefreshTokenDescriptor CreateToken();
        string HashToken(string token);
    }
}
