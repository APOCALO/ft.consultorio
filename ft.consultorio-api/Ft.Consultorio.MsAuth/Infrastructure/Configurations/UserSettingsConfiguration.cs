using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Infrastructure.Configurations
{
    public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder.ToTable("UserSettings");

            builder.HasKey(s => s.UserId);

            builder.Property(s => s.Theme)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(s => s.Language)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(s => s.NotificationsEnabled)
                .IsRequired();
        }
    }
}
