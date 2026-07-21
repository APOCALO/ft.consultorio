using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.CreatePatient
{
    public record CreatePatientCommand : BaseResponse<PatientResponseDTO>
    {
        public string Document { get; init; } = string.Empty;
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
        public Guid CreatedById { get; init; }
    }
}
