using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Repositories
{
    public class MedicalRecordRepository : BaseRepository<MedicalRecord, Guid>, IMedicalRecordRepository
    {
        private readonly ApplicationDbContext _db;

        public MedicalRecordRepository(ApplicationDbContext db) : base(db) => _db = db;

        public Task<MedicalRecord?> GetByPatientIdAsync(Guid patientId, bool asNoTracking, CancellationToken cancellationToken)
        {
            IQueryable<MedicalRecord> query = _db.MedicalRecords;
            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync(r => r.PatientId == patientId, cancellationToken);
        }

        public Task<bool> ExistsForPatientAsync(Guid patientId, CancellationToken cancellationToken) =>
            _db.MedicalRecords.AnyAsync(r => r.PatientId == patientId, cancellationToken);
    }
}
