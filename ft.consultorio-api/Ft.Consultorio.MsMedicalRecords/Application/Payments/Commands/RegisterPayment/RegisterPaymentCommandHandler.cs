using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Domain.Payments;

namespace Ft.Consultorio.MsMedicalRecords.Application.Payments.Commands.RegisterPayment
{
    internal sealed class RegisterPaymentCommandHandler : ApiBaseHandler<RegisterPaymentCommand, PaymentResponseDTO>
    {
        private readonly IPaymentRepository _payments;
        private readonly IPatientRepository _patients;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentsMapper _mapper;

        public RegisterPaymentCommandHandler(
            IPaymentRepository payments,
            IPatientRepository patients,
            IUnitOfWork unitOfWork,
            ILogger<RegisterPaymentCommandHandler> logger,
            IPaymentsMapper mapper) : base(logger)
        {
            _payments = payments;
            _patients = patients;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<PaymentResponseDTO>>> HandleRequest(
            RegisterPaymentCommand request, CancellationToken cancellationToken)
        {
            var patient = await _patients.GetByIdAsync(request.PatientId, asNoTracking: true, cancellationToken);
            if (patient is null)
            {
                return Error.NotFound("Patient.NotFound", "Patient with the provided Id was not found.");
            }

            Payment payment;
            try
            {
                payment = Payment.Create(
                    createdById: request.CreatedById,
                    patientId: request.PatientId,
                    amount: request.Amount,
                    method: request.Method,
                    sessionId: request.SessionId,
                    reference: request.Reference,
                    paidAt: request.PaidAt);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("RegisterPayment.Validation", ex.Message);
            }

            await _payments.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<PaymentResponseDTO>(_mapper.ToResponse(payment), true);
        }
    }
}
