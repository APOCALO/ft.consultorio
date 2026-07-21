using FluentValidation;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RegisterLocalUser
{
    public sealed class RegisterLocalUserCommandValidator : AbstractValidator<RegisterLocalUserCommand>
    {
        public RegisterLocalUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(256)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol.");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.BirthDate)
                .Must(date => date != default)
                .WithMessage("BirthDate is required.")
                .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("BirthDate cannot be in the future.");

            RuleFor(x => x.UserName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
        }
    }
}
