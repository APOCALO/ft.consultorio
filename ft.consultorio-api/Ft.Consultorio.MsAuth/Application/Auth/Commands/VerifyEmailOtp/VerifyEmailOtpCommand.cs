using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.VerifyEmailOtp
{
    public record VerifyEmailOtpCommand(string Email, string Otp)
        : BaseResponse<AuthTokenResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public string? RequestIp { get; init; }

        [BindNever]
        [JsonIgnore]
        public string? UserAgent { get; init; }
    }
}
