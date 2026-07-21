using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Queries.GetPatientById
{
    internal sealed class GetPatientByIdQueryHandler : ApiBaseHandler<GetPatientByIdQuery, PatientResponseDTO>
    {
        private readonly IPatientRepository _patients;
        private readonly IPatientsMapper _mapper;

        public GetPatientByIdQueryHandler(
            IPatientRepository patients,
            ILogger<GetPatientByIdQueryHandler> logger,
            IPatientsMapper mapper) : base(logger)
        {
            _patients = patients;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<PatientResponseDTO>>> HandleRequest(
            GetPatientByIdQuery request, CancellationToken cancellationToken)
        {
            var patient = await _patients.GetByIdAsync(request.Id, asNoTracking: true, cancellationToken);
            if (patient is null)
            {
                return Error.NotFound("Patient.NotFound", "Patient with the provided Id was not found.");
            }

            return new ApiResponse<PatientResponseDTO>(_mapper.ToResponse(patient), true);
        }
    }
}
