using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Repositories
{
    public class PatientRepository : BaseRepository<Patient, Guid>, IPatientRepository
    {
        private readonly ApplicationDbContext _db;

        public PatientRepository(ApplicationDbContext db) : base(db) => _db = db;

        public Task<bool> DocumentExistsAsync(string document, Guid? excludeId, CancellationToken cancellationToken) =>
            _db.Patients.AnyAsync(
                p => p.Document == document && (excludeId == null || p.Id != excludeId),
                cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken) =>
            _db.Patients.CountAsync(cancellationToken);

        public Task<int> CountByStatusAsync(PatientStatus status, CancellationToken cancellationToken) =>
            _db.Patients.CountAsync(p => p.Status == status, cancellationToken);
    }
}
