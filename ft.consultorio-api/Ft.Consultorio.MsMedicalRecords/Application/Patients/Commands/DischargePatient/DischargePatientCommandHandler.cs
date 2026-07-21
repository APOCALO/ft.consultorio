using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.DischargePatient
{
    internal sealed class DischargePatientCommandHandler : ApiBaseHandler<DischargePatientCommand, PatientResponseDTO>
    {
        private readonly IPatientRepository _patients;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPatientsMapper _mapper;

        public DischargePatientCommandHandler(
            IPatientRepository patients,
            IUnitOfWork unitOfWork,
            ILogger<DischargePatientCommandHandler> logger,
            IPatientsMapper mapper) : base(logger)
        {
            _patients = patients;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<PatientResponseDTO>>> HandleRequest(
            DischargePatientCommand request, CancellationToken cancellationToken)
        {
            var patient = await _patients.GetByIdAsync(request.Id, cancellationToken);
            if (patient is null)
            {
                return Error.NotFound("Patient.NotFound", "Patient with the provided Id was not found.");
            }

            patient.Discharge(request.UpdatedById);
            _patients.Update(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<PatientResponseDTO>(_mapper.ToResponse(patient), true);
        }
    }
}
