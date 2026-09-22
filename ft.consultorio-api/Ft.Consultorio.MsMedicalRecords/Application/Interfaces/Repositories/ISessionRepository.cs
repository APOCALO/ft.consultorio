using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Models;
using Ft.Consultorio.MsMedicalRecords.Domain.Sessions;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories
{
    public interface ISessionRepository : IBaseRepository<Session, Guid>
    {
        Task<IReadOnlyList<Session>> GetByRecordAsync(Guid medicalRecordId, CancellationToken cancellationToken);
        Task<int> CountByDateRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
        Task<decimal> SumPaidBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
        Task<decimal> SumUnpaidAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<SessionBalanceRow>> ListPaidBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
        Task<IReadOnlyList<SessionBalanceRow>> ListUnpaidAsync(CancellationToken cancellationToken);
    }
}
