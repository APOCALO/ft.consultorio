using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.CreatePatient
{
    internal sealed class CreatePatientCommandHandler : ApiBaseHandler<CreatePatientCommand, PatientResponseDTO>
    {
        private readonly IPatientRepository _patients;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPatientsMapper _mapper;

        public CreatePatientCommandHandler(
            IPatientRepository patients,
            IUnitOfWork unitOfWork,
            ILogger<CreatePatientCommandHandler> logger,
            IPatientsMapper mapper) : base(logger)
        {
            _patients = patients;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<PatientResponseDTO>>> HandleRequest(
            CreatePatientCommand request, CancellationToken cancellationToken)
        {
            if (await _patients.DocumentExistsAsync(request.Document.Trim(), null, cancellationToken))
            {
                return Error.Conflict("Patient.DuplicateDocument", "A patient with that document already exists.");
            }

            Patient patient;
            try
            {
                patient = Patient.Create(
                    createdById: request.CreatedById,
                    document: request.Document,
                    fullName: request.FullName,
                    birthDate: request.BirthDate,
                    gender: request.Gender,
                    phone: request.Phone,
                    email: request.Email,
                    instagram: request.Instagram,
                    occupation: request.Occupation,
                    address: request.Address,
                    emergencyContact: request.EmergencyContact);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("CreatePatient.Validation", ex.Message);
            }

            await _patients.AddAsync(patient, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<PatientResponseDTO>(_mapper.ToResponse(patient), true);
        }
    }
}
