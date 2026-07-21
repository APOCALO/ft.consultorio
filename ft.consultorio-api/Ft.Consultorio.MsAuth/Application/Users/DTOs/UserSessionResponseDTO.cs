using Ft.Consultorio.MsAuth.Application.Roles.DTOs;

namespace Ft.Consultorio.MsAuth.Application.Users.DTOs
{
    public sealed record UserSessionResponseDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? AvatarUrl { get; set; }
        public bool IsLocal { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsFirstLogin { get; set; }
        public List<RoleResponseDTO> Roles { get; set; } = new();
    }
}
