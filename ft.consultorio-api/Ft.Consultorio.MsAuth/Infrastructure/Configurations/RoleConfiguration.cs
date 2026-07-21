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
        }
    }
}
