using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Infrastructure.Configurations
{
    public sealed class EmailVerificationOtpConfiguration : IEntityTypeConfiguration<EmailVerificationOtp>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationOtp> builder)
        {
            builder.ToTable("EmailVerificationOtps");

            builder.HasKey(otp => otp.Id);

            builder.Property(otp => otp.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(otp => otp.UserId)
                .IsRequired();

            builder.Property(otp => otp.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(otp => otp.Purpose)
                .HasConversion<int>()
                .HasDefaultValue(OtpPurpose.Registration)
                .IsRequired();

            builder.Property(otp => otp.CodeHash)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(otp => otp.CodeSalt)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(otp => otp.ExpiresAt)
                .IsRequired();

            builder.Property(otp => otp.Attempts)
                .IsRequired();

            builder.Property(otp => otp.ConsumedAt);

            builder.HasIndex(otp => new { otp.UserId, otp.CreatedAt });
            builder.HasIndex(otp => new { otp.UserId, otp.Purpose, otp.CreatedAt });
            builder.HasIndex(otp => new { otp.Email, otp.CreatedAt });

            builder.HasOne(otp => otp.User)
                .WithMany(user => user.EmailVerificationOtps)
                .HasForeignKey(otp => otp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
