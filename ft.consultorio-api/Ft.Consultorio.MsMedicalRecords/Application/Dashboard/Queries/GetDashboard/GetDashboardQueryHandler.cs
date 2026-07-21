using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Queries.GetDashboard
{
    internal sealed class GetDashboardQueryHandler : ApiBaseHandler<GetDashboardQuery, DashboardResponseDTO>
    {
        private readonly IPatientRepository _patients;
        private readonly ISessionRepository _sessions;
        private readonly IPaymentRepository _payments;

        public GetDashboardQueryHandler(
            IPatientRepository patients,
            ISessionRepository sessions,
            IPaymentRepository payments,
            ILogger<GetDashboardQueryHandler> logger) : base(logger)
        {
            _patients = patients;
            _sessions = sessions;
            _payments = payments;
        }

        protected override async Task<ErrorOr<ApiResponse<DashboardResponseDTO>>> HandleRequest(
            GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var tomorrowStart = todayStart.AddDays(1);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonthStart = monthStart.AddMonths(1);

            var dto = new DashboardResponseDTO
            {
                Patients = await _patients.CountAsync(cancellationToken),
                ActivePatients = await _patients.CountByStatusAsync(PatientStatus.Active, cancellationToken),
                SessionsToday = await _sessions.CountByDateRangeAsync(todayStart, tomorrowStart, cancellationToken),
                IncomeThisMonth = await _payments.SumBetweenAsync(monthStart, nextMonthStart, cancellationToken),
                PendingBalance = await _sessions.SumUnpaidAsync(cancellationToken)
            };

            return new ApiResponse<DashboardResponseDTO>(dto, true);
        }
    }
}
