using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Queries.GetSessionsByRecord
{
    public record GetSessionsByRecordQuery : BaseResponse<IReadOnlyList<SessionResponseDTO>>
    {
        public Guid MedicalRecordId { get; init; }
        public GetSessionsByRecordQuery(Guid medicalRecordId) => MedicalRecordId = medicalRecordId;
    }
}
