using System.Linq.Expressions;
using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Queries.GetAllPatientsPaged
{
    internal sealed class GetAllPatientsPagedQueryHandler
        : ApiBaseHandler<GetAllPatientsPagedQuery, IReadOnlyList<PatientResponseDTO>>
    {
        private readonly IPatientRepository _patients;
        private readonly IPatientsMapper _mapper;

        public GetAllPatientsPagedQueryHandler(
            IPatientRepository patients,
            ILogger<GetAllPatientsPagedQueryHandler> logger,
            IPatientsMapper mapper) : base(logger)
        {
            _patients = patients;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<IReadOnlyList<PatientResponseDTO>>>> HandleRequest(
            GetAllPatientsPagedQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Patient, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                filter = p => p.FullName.Contains(term) || p.Document.Contains(term);
            }

            var (patients, totalCount) = await _patients.GetPagedAsync(
                request.Pagination,
                filter: filter,
                orderBy: q => q.OrderBy(p => p.FullName),
                cancellationToken: cancellationToken);

            var pagination = new PaginationMetadata
            {
                TotalCount = totalCount,
                PageSize = request.Pagination.PageSize,
                PageNumber = request.Pagination.PageNumber
            };

            var mapped = _mapper.ToResponses(patients).AsReadOnly();
            return new ApiResponse<IReadOnlyList<PatientResponseDTO>>(mapped, true, pagination);
        }
    }
}
