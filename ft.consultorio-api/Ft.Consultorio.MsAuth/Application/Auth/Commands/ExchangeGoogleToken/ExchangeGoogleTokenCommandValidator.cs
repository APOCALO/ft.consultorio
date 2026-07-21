using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ExchangeGoogleToken
{
    public sealed class ExchangeGoogleTokenCommandValidator : AbstractValidator<ExchangeGoogleTokenCommand>
    {
        public ExchangeGoogleTokenCommandValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty();
        }
    }
}
