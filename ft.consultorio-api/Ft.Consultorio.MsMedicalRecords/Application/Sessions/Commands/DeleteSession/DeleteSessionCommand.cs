using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.DeleteSession
{
    public record DeleteSessionCommand : BaseResponse<bool>
    {
        public Guid Id { get; init; }
        public DeleteSessionCommand(Guid id) => Id = id;
    }
}
