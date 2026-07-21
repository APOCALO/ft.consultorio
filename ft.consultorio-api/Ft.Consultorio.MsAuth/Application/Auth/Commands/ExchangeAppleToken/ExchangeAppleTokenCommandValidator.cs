using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeAppleToken
{
    public sealed class ExchangeAppleTokenCommandValidator : AbstractValidator<ExchangeAppleTokenCommand>
    {
        public ExchangeAppleTokenCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty();
        }
    }
}
