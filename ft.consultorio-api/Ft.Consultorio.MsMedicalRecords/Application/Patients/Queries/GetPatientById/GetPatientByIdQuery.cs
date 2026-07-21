using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Queries.GetPatientById
{
    public record GetPatientByIdQuery : BaseResponse<PatientResponseDTO>
    {
        public Guid Id { get; init; }
        public GetPatientByIdQuery(Guid id) => Id = id;
    }
}
