namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs
{
    public record MedicalRecordResponseDTO
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? CurrentIllness { get; set; }
        public string? MedicalDiagnosis { get; set; }
        public string? PhysiotherapyDiagnosis { get; set; }
        public string? ShortGoals { get; set; }
        public string? MediumGoals { get; set; }
        public string? LongGoals { get; set; }
        public string? Observations { get; set; }
        public MedicalHistoryDTO History { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
}
