using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.DTOs;
using Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Models;
using Ft.Consultorio.MsMedicalRecords.Application.Interfaces.Repositories;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Application.Dashboard.Queries.GetDashboard
{
    internal sealed class GetDashboardQueryHandler : ApiBaseHandler<GetDashboardQuery, DashboardResponseDTO>
    {
        private readonly IPatientRepository _patients;
        private readonly ISessionRepository _sessions;

        public GetDashboardQueryHandler(
            IPatientRepository patients,
            ISessionRepository sessions,
            ILogger<GetDashboardQueryHandler> logger) : base(logger)
        {
            _patients = patients;
            _sessions = sessions;
        }

        protected override async Task<ErrorOr<ApiResponse<DashboardResponseDTO>>> HandleRequest(
            GetDashboardQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var tomorrowStart = todayStart.AddDays(1);
            var (monthStart, nextMonthStart) = CurrentMonthUtc();

            var income = Group(
                await _sessions.ListPaidBetweenAsync(monthStart, nextMonthStart, cancellationToken),
                byPaidAt: true);
            var pending = Group(
                await _sessions.ListUnpaidAsync(cancellationToken),
                byPaidAt: false);

            var dto = new DashboardResponseDTO
            {
                Patients = await _patients.CountAsync(cancellationToken),
                ActivePatients = await _patients.CountByStatusAsync(PatientStatus.Active, cancellationToken),
                SessionsToday = await _sessions.CountByDateRangeAsync(todayStart, tomorrowStart, cancellationToken),
                IncomeThisMonth = income.Sum(p => p.Total),
                PendingBalance = pending.Sum(p => p.Total),
                Income = income,
                Pending = pending
            };

            return new ApiResponse<DashboardResponseDTO>(dto, true);
        }

        private static IReadOnlyList<DashboardPatientBalanceDTO> Group(
            IReadOnlyList<SessionBalanceRow> rows, bool byPaidAt) =>
            rows
                .GroupBy(r => r.PatientId)
                .Select(g =>
                {
                    var sessions = (byPaidAt
                            ? g.OrderByDescending(s => s.PaidAt).ThenByDescending(s => s.Date)
                            : g.OrderByDescending(s => s.Date))
                        .Select(s => new DashboardSessionBalanceDTO
                        {
                            SessionId = s.SessionId,
                            Date = s.Date,
                            PaidAt = s.PaidAt,
                            Price = s.Price
                        })
                        .ToList();

                    return new DashboardPatientBalanceDTO
                    {
                        PatientId = g.Key,
                        PatientName = g.First().PatientName,
                        Total = sessions.Sum(s => s.Price),
                        Sessions = sessions
                    };
                })
                .OrderByDescending(p => p.Total)
                .ThenBy(p => p.PatientName)
                .ToList();

        /// <summary>
        /// Mes calendario de Colombia. El check «sesión pagada» guarda <c>PaidAt</c> en UTC.
        /// </summary>
        private static (DateTime Start, DateTime End) CurrentMonthUtc()
        {
            var zone = TimeZoneInfo.TryFindSystemTimeZoneById("America/Bogota", out var bogota) && bogota is not null
                ? bogota
                : TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
            var startLocal = new DateTime(localNow.Year, localNow.Month, 1);
            var endLocal = startLocal.AddMonths(1);
            return (
                TimeZoneInfo.ConvertTimeToUtc(startLocal, zone),
                TimeZoneInfo.ConvertTimeToUtc(endLocal, zone));
        }
    }
}
