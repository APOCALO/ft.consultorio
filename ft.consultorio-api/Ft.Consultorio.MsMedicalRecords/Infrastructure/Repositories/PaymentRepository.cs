using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Domain.Payments;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Persistence.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Repositories
{
    public class PaymentRepository : BaseRepository<Payment, Guid>, IPaymentRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentRepository(ApplicationDbContext db) : base(db) => _db = db;

        public async Task<IReadOnlyList<Payment>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken) =>
            await _db.Payments.AsNoTracking()
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync(cancellationToken);

        public async Task<decimal> SumBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken) =>
            await _db.Payments.Where(p => p.PaidAt >= fromUtc && p.PaidAt < toUtc)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }
}
