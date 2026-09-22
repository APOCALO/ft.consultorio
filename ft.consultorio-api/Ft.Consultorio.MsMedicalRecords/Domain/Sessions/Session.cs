using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsMedicalRecords.Domain.Sessions
{
    /// <summary>Sesión (cita atendida) asociada a una historia clínica.</summary>
    public sealed class Session : AggregateRoot
    {
        public Guid MedicalRecordId { get; private set; }
        public DateTime Date { get; private set; }
        public int PainScale { get; private set; }
        public string? Evolution { get; private set; }
        public string? TreatmentPerformed { get; private set; }
        public string? Recommendations { get; private set; }
        public DateTime? NextAppointment { get; private set; }
        public decimal Price { get; private set; }
        public bool Paid { get; private set; }
        /// <summary>Momento en que se marcó pagada. Es el que cuenta como ingreso del mes.</summary>
        public DateTime? PaidAt { get; private set; }

        private Session() { }

        private Session(
            Guid createdById,
            Guid medicalRecordId,
            DateTime date,
            int painScale,
            string? evolution,
            string? treatmentPerformed,
            string? recommendations,
            DateTime? nextAppointment,
            decimal price,
            bool paid,
            Guid? id) : base(createdById, id)
        {
            MedicalRecordId = medicalRecordId;
            Date = date;
            PainScale = painScale;
            Evolution = evolution;
            TreatmentPerformed = treatmentPerformed;
            Recommendations = recommendations;
            NextAppointment = nextAppointment;
            Price = price;
            Paid = paid;
            PaidAt = paid ? DateTime.UtcNow : null;
        }

        public static Session Create(
            Guid createdById,
            Guid medicalRecordId,
            DateTime date,
            int painScale = 0,
            string? evolution = null,
            string? treatmentPerformed = null,
            string? recommendations = null,
            DateTime? nextAppointment = null,
            decimal price = 0,
            bool paid = false,
            Guid? id = null)
        {
            if (medicalRecordId == Guid.Empty)
                throw new ArgumentException("MedicalRecordId is required.", nameof(medicalRecordId));
            if (painScale < 0 || painScale > 10)
                throw new ArgumentException("PainScale must be between 0 and 10.", nameof(painScale));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            return new Session(
                createdById,
                medicalRecordId,
                ToUtc(date),
                painScale,
                Clean(evolution),
                Clean(treatmentPerformed),
                Clean(recommendations),
                ToUtc(nextAppointment),
                price,
                paid,
                id);
        }

        public void Update(
            DateTime date,
            int painScale,
            string? evolution,
            string? treatmentPerformed,
            string? recommendations,
            DateTime? nextAppointment,
            decimal price,
            bool paid,
            Guid updatedById)
        {
            if (painScale < 0 || painScale > 10)
                throw new ArgumentException("PainScale must be between 0 and 10.", nameof(painScale));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            Date = ToUtc(date);
            PainScale = painScale;
            Evolution = Clean(evolution);
            TreatmentPerformed = Clean(treatmentPerformed);
            Recommendations = Clean(recommendations);
            NextAppointment = ToUtc(nextAppointment);
            Price = price;
            if (paid)
            {
                if (!Paid || PaidAt is null)
                    PaidAt = DateTime.UtcNow;
            }
            else
            {
                PaidAt = null;
            }
            Paid = paid;
            SetAuditUpdate(updatedById);
        }

        public void MarkAsPaid(Guid updatedById)
        {
            if (!Paid || PaidAt is null)
                PaidAt = DateTime.UtcNow;
            Paid = true;
            SetAuditUpdate(updatedById);
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Normaliza a UTC antes de persistir: las columnas son
        /// `timestamp with time zone` y Npgsql rechaza `DateTimeKind.Unspecified`
        /// (es lo que llega cuando el JSON trae una fecha sin offset, p. ej. desde
        /// un &lt;input type="datetime-local"&gt;). `Local` se convierte; `Unspecified`
        /// se interpreta como UTC.
        /// </summary>
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        private static DateTime? ToUtc(DateTime? value) =>
            value.HasValue ? ToUtc(value.Value) : null;
    }
}
