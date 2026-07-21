using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Ft.Consultorio.MsMedicalRecords.Application.Sessions.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Sessions.Commands.CreateSession
{
    public record CreateSessionCommand : BaseResponse<SessionResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public Guid MedicalRecordId { get; init; }

        public DateTime Date { get; init; } = DateTime.UtcNow;
        public int PainScale { get; init; }
        public string? Evolution { get; init; }
        public string? TreatmentPerformed { get; init; }
        public string? Recommendations { get; init; }
        public DateTime? NextAppointment { get; init; }
        public decimal Price { get; init; }
        public bool Paid { get; init; }

        [BindNever]
        [JsonIgnore]
        public Guid CreatedById { get; init; }
    }
}
