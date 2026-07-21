using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Infrastructure.Configurations
{
    public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(token => token.Id);

            builder.Property(token => token.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(token => token.UserId)
                .IsRequired();

            builder.Property(token => token.TokenHash)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(token => token.ExpiresAt)
                .IsRequired();

            builder.Property(token => token.UsedAt);

            builder.Property(token => token.Attempts)
                .IsRequired();

            builder.Property(token => token.CreatedByIp)
                .HasMaxLength(45);

            builder.Property(token => token.CreatedByUserAgent)
                .HasMaxLength(300);

            builder.HasIndex(token => token.TokenHash)
                .IsUnique();
            builder.HasIndex(token => new { token.UserId, token.CreatedAt });

            builder.HasOne(token => token.User)
                .WithMany(user => user.PasswordResetTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
