namespace Ft.Consultorio.MsAuth.Application.Roles.DTOs
{
    public record RoleResponseDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Description { get; init; } = default!;
    }
}
