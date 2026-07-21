using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword)
        : BaseResponse<ResetPasswordResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public string? RequestIp { get; init; }
    }
}
