using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Infrastructure.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(rt => rt.UserId)
                .IsRequired();

            builder.Property(rt => rt.TokenHash)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.RevokedAt);

            builder.Property(rt => rt.ReplacedByTokenHash)
                .HasMaxLength(200);

            builder.Property(rt => rt.RevokedReason)
                .HasMaxLength(200);

            builder.Property(rt => rt.CreatedByIp)
                .HasMaxLength(45);

            builder.Property(rt => rt.CreatedByUserAgent)
                .HasMaxLength(300);

            builder.Property(rt => rt.RevokedByIp)
                .HasMaxLength(45);

            builder.Ignore(rt => rt.IsActive);

            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.HasIndex(rt => rt.UserId);

            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
