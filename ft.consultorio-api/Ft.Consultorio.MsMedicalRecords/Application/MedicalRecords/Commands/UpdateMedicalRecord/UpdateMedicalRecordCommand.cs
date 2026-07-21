using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.MedicalRecords.Commands.UpdateMedicalRecord
{
    public record UpdateMedicalRecordCommand : BaseResponse<MedicalRecordResponseDTO>
    {
        public Guid Id { get; init; }

        public string? ChiefComplaint { get; init; }
        public string? CurrentIllness { get; init; }
        public string? MedicalDiagnosis { get; init; }
        public string? PhysiotherapyDiagnosis { get; init; }
        public string? ShortGoals { get; init; }
        public string? MediumGoals { get; init; }
        public string? LongGoals { get; init; }
        public string? Observations { get; init; }
        public MedicalHistoryDTO? History { get; init; }

        [BindNever]
        [JsonIgnore]
        public Guid UpdatedById { get; init; }
    }
}
