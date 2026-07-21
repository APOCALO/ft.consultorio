using Riok.Mapperly.Abstractions;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.Sessions;

namespace Ft.Consultorio.MsMedicalRecords.Application.Mapping
{
    public interface ISessionsMapper
    {
        SessionResponseDTO ToResponse(Session source);
        List<SessionResponseDTO> ToResponses(IEnumerable<Session> source);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class SessionsMapper : ISessionsMapper
    {
        public partial SessionResponseDTO ToResponse(Session source);
        public partial List<SessionResponseDTO> ToResponses(IEnumerable<Session> source);
    }
}
