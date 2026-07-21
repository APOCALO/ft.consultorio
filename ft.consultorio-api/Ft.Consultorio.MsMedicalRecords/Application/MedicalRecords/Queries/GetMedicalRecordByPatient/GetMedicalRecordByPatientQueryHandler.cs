using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Queries.GetMedicalRecordByPatient
{
    internal sealed class GetMedicalRecordByPatientQueryHandler : ApiBaseHandler<GetMedicalRecordByPatientQuery, MedicalRecordResponseDTO>
    {
        private readonly IMedicalRecordRepository _records;
        private readonly IMedicalRecordsMapper _mapper;

        public GetMedicalRecordByPatientQueryHandler(
            IMedicalRecordRepository records,
            ILogger<GetMedicalRecordByPatientQueryHandler> logger,
            IMedicalRecordsMapper mapper) : base(logger)
        {
            _records = records;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<MedicalRecordResponseDTO>>> HandleRequest(
            GetMedicalRecordByPatientQuery request, CancellationToken cancellationToken)
        {
            var record = await _records.GetByPatientIdAsync(request.PatientId, asNoTracking: true, cancellationToken);
            if (record is null)
            {
                return Error.NotFound("MedicalRecord.NotFound", "The patient does not have a medical record yet.");
            }

            return new ApiResponse<MedicalRecordResponseDTO>(_mapper.ToResponse(record), true);
        }
    }
}
