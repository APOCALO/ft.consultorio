// Infraestructura/Persistencia/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsAuth.Domain;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;

namespace Ft.Consultorio.MsAuth.Infrastructure.Data
{
    public class ApplicationDbContext : EventPublishingDbContext
    {
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;
        public DbSet<UserSettings> UserSettings { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
        public DbSet<EmailVerificationOtp> EmailVerificationOtps { get; set; } = default!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = default!;

        // Único ctor público, solo con opciones (requerido por AddDbContextPool).
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
