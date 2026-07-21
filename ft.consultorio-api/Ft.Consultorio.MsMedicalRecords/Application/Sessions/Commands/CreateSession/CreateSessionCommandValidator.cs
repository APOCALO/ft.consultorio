using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.CreateSession
{
    public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
    {
        public CreateSessionCommandValidator()
        {
            RuleFor(c => c.MedicalRecordId).NotEqual(Guid.Empty);
            RuleFor(c => c.PainScale).InclusiveBetween(0, 10);
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
            RuleFor(c => c.CreatedById).NotEqual(Guid.Empty);
        }
    }
}
