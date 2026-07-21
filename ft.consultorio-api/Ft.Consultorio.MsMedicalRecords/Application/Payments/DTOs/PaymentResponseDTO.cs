using Ft.Consultorio.MsMedicalRecords.Domain.Payments;

namespace Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs
{
    public record PaymentResponseDTO
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid? SessionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string? Reference { get; set; }
        public DateTime PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
}
