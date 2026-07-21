using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Domain.Primitives;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.DeleteSession
{
    internal sealed class DeleteSessionCommandHandler : ApiBaseHandler<DeleteSessionCommand, bool>
    {
        private readonly ISessionRepository _sessions;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSessionCommandHandler(
            ISessionRepository sessions,
            IUnitOfWork unitOfWork,
            ILogger<DeleteSessionCommandHandler> logger) : base(logger)
        {
            _sessions = sessions;
            _unitOfWork = unitOfWork;
        }

        protected override async Task<ErrorOr<ApiResponse<bool>>> HandleRequest(
            DeleteSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await _sessions.GetByIdAsync(request.Id, cancellationToken);
            if (session is null)
            {
                return Error.NotFound("Session.NotFound", "Session with the provided Id was not found.");
            }

            _sessions.Delete(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>(true, true);
        }
    }
}
