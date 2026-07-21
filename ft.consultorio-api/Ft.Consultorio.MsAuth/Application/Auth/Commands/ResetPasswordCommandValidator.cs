using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .MaximumLength(2048);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(256)
                .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one symbol.");
        }
    }
}
