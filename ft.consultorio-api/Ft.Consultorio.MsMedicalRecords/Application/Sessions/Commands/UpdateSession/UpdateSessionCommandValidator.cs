using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.UpdateSession
{
    public class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
    {
        public UpdateSessionCommandValidator()
        {
            RuleFor(c => c.Id).NotEqual(Guid.Empty);
            RuleFor(c => c.PainScale).InclusiveBetween(0, 10);
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
            RuleFor(c => c.UpdatedById).NotEqual(Guid.Empty);
        }
    }
}
