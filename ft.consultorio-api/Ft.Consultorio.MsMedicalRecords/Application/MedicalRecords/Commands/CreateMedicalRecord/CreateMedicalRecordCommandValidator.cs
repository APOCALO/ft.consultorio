using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
    {
        public CreateMedicalRecordCommandValidator()
        {
            RuleFor(c => c.PatientId).NotEqual(Guid.Empty);
            RuleFor(c => c.CreatedById).NotEqual(Guid.Empty);
        }
    }
}
