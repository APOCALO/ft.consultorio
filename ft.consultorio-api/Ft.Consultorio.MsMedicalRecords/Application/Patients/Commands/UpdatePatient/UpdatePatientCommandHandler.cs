using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.UpdatePatient
{
    internal sealed class UpdatePatientCommandHandler : ApiBaseHandler<UpdatePatientCommand, PatientResponseDTO>
    {
        private readonly IPatientRepository _patients;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPatientsMapper _mapper;

        public UpdatePatientCommandHandler(
            IPatientRepository patients,
            IUnitOfWork unitOfWork,
            ILogger<UpdatePatientCommandHandler> logger,
            IPatientsMapper mapper) : base(logger)
        {
            _patients = patients;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<PatientResponseDTO>>> HandleRequest(
            UpdatePatientCommand request, CancellationToken cancellationToken)
        {
            var patient = await _patients.GetByIdAsync(request.Id, cancellationToken);
            if (patient is null)
            {
                return Error.NotFound("Patient.NotFound", "Patient with the provided Id was not found.");
            }

            try
            {
                patient.Update(
                    request.FullName, request.BirthDate, request.Gender, request.Phone,
                    request.Email, request.Instagram, request.Occupation, request.Address,
                    request.EmergencyContact, request.UpdatedById);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("UpdatePatient.Validation", ex.Message);
            }

            _patients.Update(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<PatientResponseDTO>(_mapper.ToResponse(patient), true);
        }
    }
}
