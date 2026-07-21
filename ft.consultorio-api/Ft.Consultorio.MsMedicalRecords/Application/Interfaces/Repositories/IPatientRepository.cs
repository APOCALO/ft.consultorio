using Ft.Consultorio.MsMedicalRecords.Domain.Patients;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories
{
    public interface IPatientRepository : IBaseRepository<Patient, Guid>
    {
        Task<bool> DocumentExistsAsync(string document, Guid? excludeId, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        Task<int> CountByStatusAsync(PatientStatus status, CancellationToken cancellationToken);
    }
}
