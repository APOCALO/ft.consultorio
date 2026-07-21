using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email)
        : BaseResponse<ForgotPasswordResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public string? RequestIp { get; init; }

        [BindNever]
        [JsonIgnore]
        public string? UserAgent { get; init; }
    }
}
