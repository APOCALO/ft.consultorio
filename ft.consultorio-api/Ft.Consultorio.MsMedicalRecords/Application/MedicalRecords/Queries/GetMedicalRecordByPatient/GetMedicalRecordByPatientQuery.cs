using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Queries.GetMedicalRecordByPatient
{
    public record GetMedicalRecordByPatientQuery : BaseResponse<MedicalRecordResponseDTO>
    {
        public Guid PatientId { get; init; }
        public GetMedicalRecordByPatientQuery(Guid patientId) => PatientId = patientId;
    }
}
