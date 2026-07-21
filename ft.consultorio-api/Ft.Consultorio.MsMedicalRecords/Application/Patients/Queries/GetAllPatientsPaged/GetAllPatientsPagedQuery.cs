using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Queries.GetAllPatientsPaged
{
    public record GetAllPatientsPagedQuery : BaseResponse<IReadOnlyList<PatientResponseDTO>>
    {
        public PaginationParameters Pagination { get; init; }
        public string? Search { get; init; }

        public GetAllPatientsPagedQuery(PaginationParameters pagination, string? search)
        {
            Pagination = pagination;
            Search = search;
        }
    }
}
