using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RevokeRefreshToken
{
    public sealed class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
    {
        public RevokeRefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("RefreshToken is required.");
        }
    }
}
