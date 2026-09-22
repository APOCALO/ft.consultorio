using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Models;
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

        public async Task<decimal> SumPaidBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken) =>
            await _db.Sessions
                .Where(s => s.Paid && s.PaidAt >= fromUtc && s.PaidAt < toUtc)
                .SumAsync(s => (decimal?)s.Price, cancellationToken) ?? 0m;

        public async Task<decimal> SumUnpaidAsync(CancellationToken cancellationToken) =>
            await _db.Sessions.Where(s => !s.Paid).SumAsync(s => (decimal?)s.Price, cancellationToken) ?? 0m;

        public async Task<IReadOnlyList<SessionBalanceRow>> ListPaidBetweenAsync(
            DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken) =>
            await Project(_db.Sessions.AsNoTracking()
                    .Where(s => s.Paid && s.Price > 0 && s.PaidAt >= fromUtc && s.PaidAt < toUtc))
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<SessionBalanceRow>> ListUnpaidAsync(CancellationToken cancellationToken) =>
            await Project(_db.Sessions.AsNoTracking().Where(s => !s.Paid && s.Price > 0))
                .ToListAsync(cancellationToken);

        private IQueryable<SessionBalanceRow> Project(IQueryable<Session> sessions) =>
            from s in sessions
            join r in _db.MedicalRecords.AsNoTracking() on s.MedicalRecordId equals r.Id
            join p in _db.Patients.AsNoTracking() on r.PatientId equals p.Id
            select new SessionBalanceRow(s.Id, p.Id, p.FullName, s.Date, s.PaidAt, s.Price);
    }
}
