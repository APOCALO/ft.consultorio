using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs
{
    public record PatientResponseDTO
    {
        public Guid Id { get; set; }
        public string Document { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public GenderEnum Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Instagram { get; set; }
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public PatientStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
}
