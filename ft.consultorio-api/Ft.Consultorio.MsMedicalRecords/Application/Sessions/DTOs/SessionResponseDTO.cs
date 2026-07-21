namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs
{
    public record SessionResponseDTO
    {
        public Guid Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public DateTime Date { get; set; }
        public int PainScale { get; set; }
        public string? Evolution { get; set; }
        public string? TreatmentPerformed { get; set; }
        public string? Recommendations { get; set; }
        public DateTime? NextAppointment { get; set; }
        public decimal Price { get; set; }
        public bool Paid { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
}
