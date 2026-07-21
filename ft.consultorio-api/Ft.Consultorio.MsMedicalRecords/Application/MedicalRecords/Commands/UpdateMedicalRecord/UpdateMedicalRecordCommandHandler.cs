using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.UpdateMedicalRecord
{
    internal sealed class UpdateMedicalRecordCommandHandler : ApiBaseHandler<UpdateMedicalRecordCommand, MedicalRecordResponseDTO>
    {
        private readonly IMedicalRecordRepository _records;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMedicalRecordsMapper _mapper;

        public UpdateMedicalRecordCommandHandler(
            IMedicalRecordRepository records,
            IUnitOfWork unitOfWork,
            ILogger<UpdateMedicalRecordCommandHandler> logger,
            IMedicalRecordsMapper mapper) : base(logger)
        {
            _records = records;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<MedicalRecordResponseDTO>>> HandleRequest(
            UpdateMedicalRecordCommand request, CancellationToken cancellationToken)
        {
            var record = await _records.GetByIdAsync(request.Id, cancellationToken);
            if (record is null)
            {
                return Error.NotFound("MedicalRecord.NotFound", "Medical record with the provided Id was not found.");
            }

            var history = request.History is null
                ? MedicalHistory.Empty()
                : new MedicalHistory(
                    request.History.Hypertension, request.History.Diabetes, request.History.Cancer,
                    request.History.Pacemaker, request.History.Pregnancy, request.History.Surgeries,
                    request.History.Fractures, request.History.Medications, request.History.Allergies,
                    request.History.OtherHistory);

            record.Update(
                request.ChiefComplaint, request.CurrentIllness, request.MedicalDiagnosis,
                request.PhysiotherapyDiagnosis, request.ShortGoals, request.MediumGoals,
                request.LongGoals, request.Observations, history, request.UpdatedById);

            _records.Update(record);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<MedicalRecordResponseDTO>(_mapper.ToResponse(record), true);
        }
    }
}
