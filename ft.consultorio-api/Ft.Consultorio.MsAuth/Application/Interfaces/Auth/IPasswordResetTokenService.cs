namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IPasswordResetTokenService
    {
        PasswordResetTokenDescriptor CreateToken();
        string HashToken(string token);
    }
}
