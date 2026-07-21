namespace Ft.Consultorio.MsMedicalRecords.Application.Dashboard.DTOs
{
    public record DashboardResponseDTO
    {
        public int Patients { get; set; }
        public int ActivePatients { get; set; }
        public int SessionsToday { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public decimal PendingBalance { get; set; }
    }
}
