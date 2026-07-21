using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories
{
    public interface IMedicalRecordRepository : IBaseRepository<MedicalRecord, Guid>
    {
        Task<MedicalRecord?> GetByPatientIdAsync(Guid patientId, bool asNoTracking, CancellationToken cancellationToken);
        Task<bool> ExistsForPatientAsync(Guid patientId, CancellationToken cancellationToken);
    }
}
