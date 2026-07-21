using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ResendEmailOtp
{
    public sealed class ResendEmailOtpCommandValidator : AbstractValidator<ResendEmailOtpCommand>
    {
        public ResendEmailOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
