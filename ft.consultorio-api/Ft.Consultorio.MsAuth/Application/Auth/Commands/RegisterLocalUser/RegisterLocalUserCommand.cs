using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.MsAuth.Application.Auth.DTOs;

namespace Ft.Consultorio.MsAuth.Application.Auth.Commands.RegisterLocalUser
{
    public record RegisterLocalUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        DateOnly BirthDate,
        string? UserName = null,
        string? AvatarUrl = null,
        string? CaptchaToken = null) : BaseResponse<RegisterLocalUserPendingResponseDTO>
    {
        [BindNever]
        [JsonIgnore]
        public string? RequestIp { get; init; }

        [BindNever]
        [JsonIgnore]
        public string? UserAgent { get; init; }
    }
}
