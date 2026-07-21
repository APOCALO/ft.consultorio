namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public interface IEmailVerificationOtpService
    {
        EmailVerificationOtpDescriptor CreateOtp();
        bool VerifyCode(string code, string codeSalt, string expectedCodeHash);
    }
}
