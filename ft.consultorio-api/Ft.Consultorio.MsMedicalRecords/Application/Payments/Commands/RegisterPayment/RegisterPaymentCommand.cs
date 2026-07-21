using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using Ft.Consultorio.MsMedicalRecords.Application.Payments.DTOs;
using Ft.Consultorio.MsMedicalRecords.Domain.Payments;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsMedicalRecords.Application.Payments.Commands.RegisterPayment
{
    public record RegisterPaymentCommand : BaseResponse<PaymentResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public Guid PatientId { get; init; }

        public Guid? SessionId { get; init; }
        public decimal Amount { get; init; }
        public PaymentMethod Method { get; init; } = PaymentMethod.Cash;
        public string? Reference { get; init; }
        public DateTime? PaidAt { get; init; }

        [BindNever]
        [JsonIgnore]
        public Guid CreatedById { get; init; }
    }
}
