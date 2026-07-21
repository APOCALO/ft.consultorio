using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.UpdatePatient
{
    public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
    {
        public UpdatePatientCommandValidator()
        {
            RuleFor(c => c.Id).NotEqual(Guid.Empty);
            RuleFor(c => c.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200);
            RuleFor(c => c.Email)
                .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email));
            RuleFor(c => c.UpdatedById).NotEqual(Guid.Empty);
        }
    }
}
