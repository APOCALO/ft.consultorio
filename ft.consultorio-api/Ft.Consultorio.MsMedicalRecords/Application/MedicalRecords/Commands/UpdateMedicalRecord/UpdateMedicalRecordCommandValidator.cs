using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommandValidator : AbstractValidator<UpdateMedicalRecordCommand>
    {
        public UpdateMedicalRecordCommandValidator()
        {
            RuleFor(c => c.Id).NotEqual(Guid.Empty);
            RuleFor(c => c.UpdatedById).NotEqual(Guid.Empty);
        }
    }
}
