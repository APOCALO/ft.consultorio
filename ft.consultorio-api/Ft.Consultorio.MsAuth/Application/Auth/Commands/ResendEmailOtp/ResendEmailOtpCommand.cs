using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ResendEmailOtp
{
    public record ResendEmailOtpCommand(string Email)
        : BaseResponse<ResendEmailOtpResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public string? RequestIp { get; init; }
    }
}
