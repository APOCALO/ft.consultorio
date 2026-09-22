namespace Ft.Consultorio.MsMedicalRecords.Application.Dashboard.DTOs
{
    public record DashboardPatientBalanceDTO
    {
        public Guid PatientId { get; init; }
        public string PatientName { get; init; } = "";
        public decimal Total { get; init; }
        public IReadOnlyList<DashboardSessionBalanceDTO> Sessions { get; init; } = [];
    }

    public record DashboardSessionBalanceDTO
    {
        public Guid SessionId { get; init; }
        public DateTime Date { get; init; }
        public DateTime? PaidAt { get; init; }
        public decimal Price { get; init; }
    }
}
