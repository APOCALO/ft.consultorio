namespace Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords
{
    /// <summary>
    /// Antecedentes del paciente. Modelado como Value Object propio (owned) del
    /// <see cref="MedicalRecord"/>: se persiste en la misma tabla.
    /// </summary>
    public sealed class MedicalHistory
    {
        public bool Hypertension { get; private set; }
        public bool Diabetes { get; private set; }
        public bool Cancer { get; private set; }
        public bool Pacemaker { get; private set; }
        public bool Pregnancy { get; private set; }
        public string? Surgeries { get; private set; }
        public string? Fractures { get; private set; }
        public string? Medications { get; private set; }
        public string? Allergies { get; private set; }
        public string? OtherHistory { get; private set; }

        private MedicalHistory() { }

        public MedicalHistory(
            bool hypertension = false,
            bool diabetes = false,
            bool cancer = false,
            bool pacemaker = false,
            bool pregnancy = false,
            string? surgeries = null,
            string? fractures = null,
            string? medications = null,
            string? allergies = null,
            string? otherHistory = null)
        {
            Hypertension = hypertension;
            Diabetes = diabetes;
            Cancer = cancer;
            Pacemaker = pacemaker;
            Pregnancy = pregnancy;
            Surgeries = string.IsNullOrWhiteSpace(surgeries) ? null : surgeries.Trim();
            Fractures = string.IsNullOrWhiteSpace(fractures) ? null : fractures.Trim();
            Medications = string.IsNullOrWhiteSpace(medications) ? null : medications.Trim();
            Allergies = string.IsNullOrWhiteSpace(allergies) ? null : allergies.Trim();
            OtherHistory = string.IsNullOrWhiteSpace(otherHistory) ? null : otherHistory.Trim();
        }

        public static MedicalHistory Empty() => new MedicalHistory();
    }
}
