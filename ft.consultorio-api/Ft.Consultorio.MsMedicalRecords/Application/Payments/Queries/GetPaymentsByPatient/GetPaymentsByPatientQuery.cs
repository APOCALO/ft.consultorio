using Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Payments.Queries.GetPaymentsByPatient
{
    public record GetPaymentsByPatientQuery : BaseResponse<IReadOnlyList<PaymentResponseDTO>>
    {
        public Guid PatientId { get; init; }
        public GetPaymentsByPatientQuery(Guid patientId) => PatientId = patientId;
    }
}
