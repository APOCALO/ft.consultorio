using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.LoginLocalUser
{
    public sealed class LoginLocalUserCommandValidator : AbstractValidator<LoginLocalUserCommand>
    {
        public LoginLocalUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
