using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords
{
    /// <summary>Historia clínica del paciente. Solo una por paciente.</summary>
    public sealed class MedicalRecord : AggregateRoot
    {
        public Guid PatientId { get; private set; }
        public string? ChiefComplaint { get; private set; }
        public string? CurrentIllness { get; private set; }
        public string? MedicalDiagnosis { get; private set; }
        public string? PhysiotherapyDiagnosis { get; private set; }
        public string? ShortGoals { get; private set; }
        public string? MediumGoals { get; private set; }
        public string? LongGoals { get; private set; }
        public string? Observations { get; private set; }
        public MedicalHistory History { get; private set; } = MedicalHistory.Empty();

        private MedicalRecord() { }

        private MedicalRecord(
            Guid createdById,
            Guid patientId,
            string? chiefComplaint,
            string? currentIllness,
            string? medicalDiagnosis,
            string? physiotherapyDiagnosis,
            string? shortGoals,
            string? mediumGoals,
            string? longGoals,
            string? observations,
            MedicalHistory history,
            Guid? id) : base(createdById, id)
        {
            PatientId = patientId;
            ChiefComplaint = chiefComplaint;
            CurrentIllness = currentIllness;
            MedicalDiagnosis = medicalDiagnosis;
            PhysiotherapyDiagnosis = physiotherapyDiagnosis;
            ShortGoals = shortGoals;
            MediumGoals = mediumGoals;
            LongGoals = longGoals;
            Observations = observations;
            History = history ?? MedicalHistory.Empty();
        }

        public static MedicalRecord Create(
            Guid createdById,
            Guid patientId,
            string? chiefComplaint = null,
            string? currentIllness = null,
            string? medicalDiagnosis = null,
            string? physiotherapyDiagnosis = null,
            string? shortGoals = null,
            string? mediumGoals = null,
            string? longGoals = null,
            string? observations = null,
            MedicalHistory? history = null,
            Guid? id = null)
        {
            if (patientId == Guid.Empty)
                throw new ArgumentException("PatientId is required.", nameof(patientId));

            return new MedicalRecord(
                createdById,
                patientId,
                Clean(chiefComplaint),
                Clean(currentIllness),
                Clean(medicalDiagnosis),
                Clean(physiotherapyDiagnosis),
                Clean(shortGoals),
                Clean(mediumGoals),
                Clean(longGoals),
                Clean(observations),
                history ?? MedicalHistory.Empty(),
                id);
        }

        public void Update(
            string? chiefComplaint,
            string? currentIllness,
            string? medicalDiagnosis,
            string? physiotherapyDiagnosis,
            string? shortGoals,
            string? mediumGoals,
            string? longGoals,
            string? observations,
            MedicalHistory? history,
            Guid updatedById)
        {
            ChiefComplaint = Clean(chiefComplaint);
            CurrentIllness = Clean(currentIllness);
            MedicalDiagnosis = Clean(medicalDiagnosis);
            PhysiotherapyDiagnosis = Clean(physiotherapyDiagnosis);
            ShortGoals = Clean(shortGoals);
            MediumGoals = Clean(mediumGoals);
            LongGoals = Clean(longGoals);
            Observations = Clean(observations);
            if (history is not null) History = history;
            SetAuditUpdate(updatedById);
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
