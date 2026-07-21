using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.Payments.Queries.GetPaymentsByPatient
{
    internal sealed class GetPaymentsByPatientQueryHandler
        : ApiBaseHandler<GetPaymentsByPatientQuery, IReadOnlyList<PaymentResponseDTO>>
    {
        private readonly IPaymentRepository _payments;
        private readonly IPaymentsMapper _mapper;

        public GetPaymentsByPatientQueryHandler(
            IPaymentRepository payments,
            ILogger<GetPaymentsByPatientQueryHandler> logger,
            IPaymentsMapper mapper) : base(logger)
        {
            _payments = payments;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<IReadOnlyList<PaymentResponseDTO>>>> HandleRequest(
            GetPaymentsByPatientQuery request, CancellationToken cancellationToken)
        {
            var payments = await _payments.GetByPatientAsync(request.PatientId, cancellationToken);
            var mapped = _mapper.ToResponses(payments).AsReadOnly();
            return new ApiResponse<IReadOnlyList<PaymentResponseDTO>>(mapped, true);
        }
    }
}
