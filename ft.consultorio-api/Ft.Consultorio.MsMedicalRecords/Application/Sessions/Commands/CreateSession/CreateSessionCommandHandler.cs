using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;
using Ft.Consultorio.MsMedicalRecords.Domain.Sessions;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.CreateSession
{
    internal sealed class CreateSessionCommandHandler : ApiBaseHandler<CreateSessionCommand, SessionResponseDTO>
    {
        private readonly ISessionRepository _sessions;
        private readonly IMedicalRecordRepository _records;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionsMapper _mapper;

        public CreateSessionCommandHandler(
            ISessionRepository sessions,
            IMedicalRecordRepository records,
            IUnitOfWork unitOfWork,
            ILogger<CreateSessionCommandHandler> logger,
            ISessionsMapper mapper) : base(logger)
        {
            _sessions = sessions;
            _records = records;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<SessionResponseDTO>>> HandleRequest(
            CreateSessionCommand request, CancellationToken cancellationToken)
        {
            var record = await _records.GetByIdAsync(request.MedicalRecordId, asNoTracking: true, cancellationToken);
            if (record is null)
            {
                return Error.NotFound("MedicalRecord.NotFound", "Medical record with the provided Id was not found.");
            }

            Session session;
            try
            {
                session = Session.Create(
                    createdById: request.CreatedById,
                    medicalRecordId: request.MedicalRecordId,
                    date: request.Date,
                    painScale: request.PainScale,
                    evolution: request.Evolution,
                    treatmentPerformed: request.TreatmentPerformed,
                    recommendations: request.Recommendations,
                    nextAppointment: request.NextAppointment,
                    price: request.Price,
                    paid: request.Paid);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("CreateSession.Validation", ex.Message);
            }

            await _sessions.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<SessionResponseDTO>(_mapper.ToResponse(session), true);
        }
    }
}
