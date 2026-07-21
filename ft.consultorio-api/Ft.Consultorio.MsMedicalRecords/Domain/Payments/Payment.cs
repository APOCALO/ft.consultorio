using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsMedicalRecords.Domain.Payments
{
    /// <summary>Pago registrado para un paciente, opcionalmente ligado a una sesión.</summary>
    public sealed class Payment : AggregateRoot
    {
        public Guid PatientId { get; private set; }
        public Guid? SessionId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentMethod Method { get; private set; }
        public string? Reference { get; private set; }
        public DateTime PaidAt { get; private set; }

        private Payment() { }

        private Payment(
            Guid createdById,
            Guid patientId,
            Guid? sessionId,
            decimal amount,
            PaymentMethod method,
            string? reference,
            DateTime paidAt,
            Guid? id) : base(createdById, id)
        {
            PatientId = patientId;
            SessionId = sessionId;
            Amount = amount;
            Method = method;
            Reference = reference;
            PaidAt = paidAt;
        }

        public static Payment Create(
            Guid createdById,
            Guid patientId,
            decimal amount,
            PaymentMethod method,
            Guid? sessionId = null,
            string? reference = null,
            DateTime? paidAt = null,
            Guid? id = null)
        {
            if (patientId == Guid.Empty)
                throw new ArgumentException("PatientId is required.", nameof(patientId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            return new Payment(
                createdById,
                patientId,
                sessionId,
                amount,
                method,
                string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
                DateTime.SpecifyKind(paidAt ?? DateTime.UtcNow, DateTimeKind.Utc),
                id);
        }
    }
}
