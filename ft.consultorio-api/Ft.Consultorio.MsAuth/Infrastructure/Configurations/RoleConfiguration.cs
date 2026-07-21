using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasMaxLength(250)
                .IsRequired();

            // Rol Admin sembrado con un Id fijo. NO se asigna a ningún usuario
            // automáticamente: la asignación (fila en UserRoles) se hace a mano
            // en la base de datos. MsMedicalRecords exige [Authorize(Roles = "Admin")].
            builder.HasData(new
            {
                Id = AdminRoleId,
                Name = "Admin",
                Description = "Administrador del consultorio con acceso total.",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedById = AdminRoleId,
            });
        }

        /// <summary>Identificador estable del rol Admin sembrado.</summary>
        public static readonly Guid AdminRoleId = new("a0000000-0000-0000-0000-000000000001");
    }
}
