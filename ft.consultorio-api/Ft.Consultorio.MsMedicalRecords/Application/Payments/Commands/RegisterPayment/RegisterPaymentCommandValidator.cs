using FluentValidation;

namespace Ft.Consultorio.MsMedicalRecords.Application.Payments.Commands.RegisterPayment
{
    public class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
    {
        public RegisterPaymentCommandValidator()
        {
            RuleFor(c => c.PatientId).NotEqual(Guid.Empty);
            RuleFor(c => c.Amount).GreaterThan(0);
            RuleFor(c => c.CreatedById).NotEqual(Guid.Empty);
        }
    }
}
