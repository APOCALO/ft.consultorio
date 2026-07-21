using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.DeletePatient
{
    public record DeletePatientCommand : BaseResponse<bool>
    {
        public Guid Id { get; init; }
        public DeletePatientCommand(Guid id) => Id = id;
    }
}
