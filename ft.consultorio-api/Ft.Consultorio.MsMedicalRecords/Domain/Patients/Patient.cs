using Ft.Consultorio.ServiceDefaults.Domain.Primitives;

namespace Ft.Consultorio.MsMedicalRecords.Domain.Patients
{
    public sealed class Patient : AggregateRoot
    {
        public string Document { get; private set; } = default!;
        public string FullName { get; private set; } = default!;
        public DateOnly? BirthDate { get; private set; }
        public GenderEnum Gender { get; private set; } = GenderEnum.Unspecified;
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public string? Instagram { get; private set; }
        public string? Occupation { get; private set; }
        public string? Address { get; private set; }
        public string? EmergencyContact { get; private set; }
        public PatientStatus Status { get; private set; } = PatientStatus.Active;

        private Patient() { }

        private Patient(
            Guid createdById,
            string document,
            string fullName,
            DateOnly? birthDate,
            GenderEnum gender,
            string? phone,
            string? email,
            string? instagram,
            string? occupation,
            string? address,
            string? emergencyContact,
            Guid? id) : base(createdById, id)
        {
            Document = document;
            FullName = fullName;
            BirthDate = birthDate;
            Gender = gender;
            Phone = phone;
            Email = email;
            Instagram = instagram;
            Occupation = occupation;
            Address = address;
            EmergencyContact = emergencyContact;
            Status = PatientStatus.Active;
        }

        public static Patient Create(
            Guid createdById,
            string document,
            string fullName,
            DateOnly? birthDate = null,
            GenderEnum gender = GenderEnum.Unspecified,
            string? phone = null,
            string? email = null,
            string? instagram = null,
            string? occupation = null,
            string? address = null,
            string? emergencyContact = null,
            Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(document))
                throw new ArgumentException("Patient document is required.", nameof(document));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Patient full name is required.", nameof(fullName));

            return new Patient(
                createdById,
                document.Trim(),
                fullName.Trim(),
                birthDate,
                gender,
                Clean(phone),
                Clean(email),
                Clean(instagram),
                Clean(occupation),
                Clean(address),
                Clean(emergencyContact),
                id);
        }

        public void Update(
            string fullName,
            DateOnly? birthDate,
            GenderEnum gender,
            string? phone,
            string? email,
            string? instagram,
            string? occupation,
            string? address,
            string? emergencyContact,
            Guid updatedById)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Patient full name is required.", nameof(fullName));

            FullName = fullName.Trim();
            BirthDate = birthDate;
            Gender = gender;
            Phone = Clean(phone);
            Email = Clean(email);
            Instagram = Clean(instagram);
            Occupation = Clean(occupation);
            Address = Clean(address);
            EmergencyContact = Clean(emergencyContact);
            SetAuditUpdate(updatedById);
        }

        public void Discharge(Guid updatedById)
        {
            Status = PatientStatus.Discharged;
            SetAuditUpdate(updatedById);
        }

        public void Activate(Guid updatedById)
        {
            Status = PatientStatus.Active;
            SetAuditUpdate(updatedById);
        }

        public void Deactivate(Guid updatedById)
        {
            Status = PatientStatus.Inactive;
            SetAuditUpdate(updatedById);
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
