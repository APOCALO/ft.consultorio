using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Domain.Sessions;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Repositories
{
    public class SessionRepository : BaseRepository<Session, Guid>, ISessionRepository
    {
        private readonly ApplicationDbContext _db;

        public SessionRepository(ApplicationDbContext db) : base(db) => _db = db;

        public async Task<IReadOnlyList<Session>> GetByRecordAsync(Guid medicalRecordId, CancellationToken cancellationToken) =>
            await _db.Sessions.AsNoTracking()
                .Where(s => s.MedicalRecordId == medicalRecordId)
                .OrderByDescending(s => s.Date)
                .ToListAsync(cancellationToken);

        public Task<int> CountByDateRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken) =>
            _db.Sessions.CountAsync(s => s.Date >= fromUtc && s.Date < toUtc, cancellationToken);

        public async Task<decimal> SumUnpaidAsync(CancellationToken cancellationToken) =>
            await _db.Sessions.Where(s => !s.Paid).SumAsync(s => (decimal?)s.Price, cancellationToken) ?? 0m;
    }
}
