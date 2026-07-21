using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.CreateMedicalRecord
{
    internal sealed class CreateMedicalRecordCommandHandler : ApiBaseHandler<CreateMedicalRecordCommand, MedicalRecordResponseDTO>
    {
        private readonly IMedicalRecordRepository _records;
        private readonly IPatientRepository _patients;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMedicalRecordsMapper _mapper;

        public CreateMedicalRecordCommandHandler(
            IMedicalRecordRepository records,
            IPatientRepository patients,
            IUnitOfWork unitOfWork,
            ILogger<CreateMedicalRecordCommandHandler> logger,
            IMedicalRecordsMapper mapper) : base(logger)
        {
            _records = records;
            _patients = patients;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<MedicalRecordResponseDTO>>> HandleRequest(
            CreateMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            var patient = await _patients.GetByIdAsync(request.PatientId, asNoTracking: true, cancellationToken);
            if (patient is null)
            {
                return Error.NotFound("Patient.NotFound", "Patient with the provided Id was not found.");
            }

            if (await _records.ExistsForPatientAsync(request.PatientId, cancellationToken))
            {
                return Error.Conflict("MedicalRecord.AlreadyExists", "The patient already has a medical record.");
            }

            var history = request.History is null
                ? MedicalHistory.Empty()
                : new MedicalHistory(
                    request.History.Hypertension, request.History.Diabetes, request.History.Cancer,
                    request.History.Pacemaker, request.History.Pregnancy, request.History.Surgeries,
                    request.History.Fractures, request.History.Medications, request.History.Allergies,
                    request.History.OtherHistory);

            MedicalRecord record;
            try
            {
                record = MedicalRecord.Create(
                    createdById: request.CreatedById,
                    patientId: request.PatientId,
                    chiefComplaint: request.ChiefComplaint,
                    currentIllness: request.CurrentIllness,
                    medicalDiagnosis: request.MedicalDiagnosis,
                    physiotherapyDiagnosis: request.PhysiotherapyDiagnosis,
                    shortGoals: request.ShortGoals,
                    mediumGoals: request.MediumGoals,
                    longGoals: request.LongGoals,
                    observations: request.Observations,
                    history: history);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("CreateMedicalRecord.Validation", ex.Message);
            }

            await _records.AddAsync(record, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<MedicalRecordResponseDTO>(_mapper.ToResponse(record), true);
        }
    }
}
