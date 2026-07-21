using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsMedicalRecords.Domain.Patients;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Configurations
{
    public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

            builder.Property(x => x.Document).HasMaxLength(30).IsRequired();
            builder.HasIndex(x => x.Document).IsUnique();

            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.BirthDate).HasColumnType("date");
            builder.Property(x => x.Gender).HasConversion<int>().HasDefaultValue(GenderEnum.Unspecified).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(30);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.Instagram).HasMaxLength(100);
            builder.Property(x => x.Occupation).HasMaxLength(100);
            builder.Property(x => x.Address).HasMaxLength(300);
            builder.Property(x => x.EmergencyContact).HasMaxLength(200);
            builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(PatientStatus.Active).IsRequired();
        }
    }
}
