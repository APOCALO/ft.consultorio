using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsAuth.Domain;

namespace Ft.Consultorio.MsAuth.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(u => u.AuthUserId)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(500);

            builder.Property(u => u.UserName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.UserEmail)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.BirthDate)
                .HasColumnType("date");

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(u => u.CountryCode)
                .HasMaxLength(4);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(15);

            builder.Property(u => u.Gender)
                .HasConversion<int>()
                .HasDefaultValue(GenderEnum.Unspecified)
                .IsRequired();

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.Property(u => u.IsEmailVerified)
                .IsRequired();

            builder.Property(u => u.LastLoginAt);

            builder.Ignore(u => u.FullName);

            builder.HasIndex(u => u.AuthUserId).IsUnique();
            builder.HasIndex(u => u.UserEmail).IsUnique();

            builder.HasMany(u => u.Roles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.EmailVerificationOtps)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.PasswordResetTokens)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Settings)
                .WithOne()
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
