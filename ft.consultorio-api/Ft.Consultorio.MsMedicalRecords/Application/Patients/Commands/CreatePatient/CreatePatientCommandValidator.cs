using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.CreatePatient
{
    public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientCommandValidator()
        {
            RuleFor(c => c.Document)
                .NotEmpty().WithMessage("Document is required.")
                .MaximumLength(30).WithMessage("Document cannot exceed 30 characters.");

            RuleFor(c => c.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

            RuleFor(c => c.Email)
                .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email))
                .WithMessage("Email is not valid.");

            RuleFor(c => c.Phone).MaximumLength(30);
            RuleFor(c => c.CreatedById)
                .NotEqual(Guid.Empty).WithMessage("User Id cannot be Guid.Empty.");
        }
    }
}
