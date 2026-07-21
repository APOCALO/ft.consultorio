using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.UpdateSession
{
    internal sealed class UpdateSessionCommandHandler : ApiBaseHandler<UpdateSessionCommand, SessionResponseDTO>
    {
        private readonly ISessionRepository _sessions;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionsMapper _mapper;

        public UpdateSessionCommandHandler(
            ISessionRepository sessions,
            IUnitOfWork unitOfWork,
            ILogger<UpdateSessionCommandHandler> logger,
            ISessionsMapper mapper) : base(logger)
        {
            _sessions = sessions;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<SessionResponseDTO>>> HandleRequest(
            UpdateSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await _sessions.GetByIdAsync(request.Id, cancellationToken);
            if (session is null)
            {
                return Error.NotFound("Session.NotFound", "Session with the provided Id was not found.");
            }

            try
            {
                session.Update(
                    request.Date, request.PainScale, request.Evolution, request.TreatmentPerformed,
                    request.Recommendations, request.NextAppointment, request.Price, request.Paid,
                    request.UpdatedById);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("UpdateSession.Validation", ex.Message);
            }

            _sessions.Update(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<SessionResponseDTO>(_mapper.ToResponse(session), true);
        }
    }
}
