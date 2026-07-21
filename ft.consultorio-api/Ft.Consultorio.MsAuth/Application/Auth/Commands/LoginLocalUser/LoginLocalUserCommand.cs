using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.LoginLocalUser
{
    public record LoginLocalUserCommand(string Email, string Password, string? CaptchaToken = null) : BaseResponse<AuthTokenResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public string? RequestIp { get; init; }

        [BindNever]
        [JsonIgnore]
        public string? UserAgent { get; init; }
    }
}
