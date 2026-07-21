using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.VerifyEmailOtp
{
    public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
    {
        public VerifyEmailOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Otp)
                .NotEmpty()
                .Length(4, 8)
                .Matches("^[0-9]+$")
                .WithMessage("OTP must contain only digits.");
        }
    }
}
