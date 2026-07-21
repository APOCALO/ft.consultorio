using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Application.Mapping;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Queries.GetSessionsByRecord
{
    internal sealed class GetSessionsByRecordQueryHandler
        : ApiBaseHandler<GetSessionsByRecordQuery, IReadOnlyList<SessionResponseDTO>>
    {
        private readonly ISessionRepository _sessions;
        private readonly ISessionsMapper _mapper;

        public GetSessionsByRecordQueryHandler(
            ISessionRepository sessions,
            ILogger<GetSessionsByRecordQueryHandler> logger,
            ISessionsMapper mapper) : base(logger)
        {
            _sessions = sessions;
            _mapper = mapper;
        }

        protected override async Task<ErrorOr<ApiResponse<IReadOnlyList<SessionResponseDTO>>>> HandleRequest(
            GetSessionsByRecordQuery request, CancellationToken cancellationToken)
        {
            var sessions = await _sessions.GetByRecordAsync(request.MedicalRecordId, cancellationToken);
            var mapped = _mapper.ToResponses(sessions).AsReadOnly();
            return new ApiResponse<IReadOnlyList<SessionResponseDTO>>(mapped, true);
        }
    }
}
