namespace Ft.Consultorio.MsAuth.Application.Interfaces.Auth
{
    public sealed record EmailVerificationOtpDescriptor(
        string Code,
        string CodeHash,
        string CodeSalt,
        DateTimeOffset ExpiresAt);
}
