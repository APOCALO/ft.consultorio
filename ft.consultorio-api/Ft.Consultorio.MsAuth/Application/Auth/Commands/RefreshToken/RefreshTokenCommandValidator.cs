using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("RefreshToken is required.");
        }
    }
}
