using Ft.Consultorio.MsMedicalRecords.Domain.Payments;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces.Repositories;

namespace Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories
{
    public interface IPaymentRepository : IBaseRepository<Payment, Guid>
    {
        Task<IReadOnlyList<Payment>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken);
        Task<decimal> SumBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
    }
}
