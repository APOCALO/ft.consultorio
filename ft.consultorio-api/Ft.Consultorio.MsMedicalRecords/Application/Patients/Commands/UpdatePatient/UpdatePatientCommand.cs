using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.UpdatePatient
{
    public record UpdatePatientCommand : BaseResponse<PatientResponseDTO>
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public DateOnly? BirthDate { get; init; }
        public GenderEnum Gender { get; init; } = GenderEnum.Unspecified;
        public string? Phone { get; init; }
        public string? Email { get; init; }
        public string? Instagram { get; init; }
        public string? Occupation { get; init; }
        public string? Address { get; init; }
        public string? EmergencyContact { get; init; }

        [BindNever]
        [JsonIgnore]
        public Guid UpdatedById { get; init; }
    }
}
