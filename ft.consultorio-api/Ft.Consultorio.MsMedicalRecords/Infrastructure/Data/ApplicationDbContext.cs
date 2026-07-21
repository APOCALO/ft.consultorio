using Microsoft.EntityFrameworkCore;
using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;
using Ft.Consultorio.MsMedicalRecords.Domain.Payments;
using Ft.Consultorio.MsMedicalRecords.Domain.Sessions;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Data
{
    public class ApplicationDbContext : EventPublishingDbContext
    {
        public DbSet<Patient> Patients { get; set; } = default!;
        public DbSet<MedicalRecord> MedicalRecords { get; set; } = default!;
        public DbSet<Session> Sessions { get; set; } = default!;
        public DbSet<Payment> Payments { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
