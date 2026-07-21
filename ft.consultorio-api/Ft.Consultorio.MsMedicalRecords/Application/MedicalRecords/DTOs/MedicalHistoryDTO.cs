namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs
{
    public record MedicalHistoryDTO
    {
        public bool Hypertension { get; set; }
        public bool Diabetes { get; set; }
        public bool Cancer { get; set; }
        public bool Pacemaker { get; set; }
        public bool Pregnancy { get; set; }
        public string? Surgeries { get; set; }
        public string? Fractures { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }
        public string? OtherHistory { get; set; }
    }
}
