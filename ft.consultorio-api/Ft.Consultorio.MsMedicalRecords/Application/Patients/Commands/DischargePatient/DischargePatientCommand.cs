using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Ft.Consultorio.MsMedicalRecords.Application.Patients.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Patients.Commands.DischargePatient
{
    public record DischargePatientCommand : BaseResponse<PatientResponseDTO>
    {
        public Guid Id { get; init; }

        [BindNever]
        [JsonIgnore]
        public Guid UpdatedById { get; init; }
    }
}
