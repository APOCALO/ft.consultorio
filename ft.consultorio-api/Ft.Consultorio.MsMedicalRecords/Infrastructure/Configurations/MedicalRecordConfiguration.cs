using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsMedicalRecords.Domain.MedicalRecords;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Configurations
{
    public sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("MedicalRecords");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

            builder.Property(x => x.PatientId).IsRequired();
            // Una historia clínica por paciente.
            builder.HasIndex(x => x.PatientId).IsUnique();

            builder.Property(x => x.ChiefComplaint).HasMaxLength(2000);
            builder.Property(x => x.CurrentIllness).HasMaxLength(4000);
            builder.Property(x => x.MedicalDiagnosis).HasMaxLength(2000);
            builder.Property(x => x.PhysiotherapyDiagnosis).HasMaxLength(2000);
            builder.Property(x => x.ShortGoals).HasMaxLength(2000);
            builder.Property(x => x.MediumGoals).HasMaxLength(2000);
            builder.Property(x => x.LongGoals).HasMaxLength(2000);
            builder.Property(x => x.Observations).HasMaxLength(4000);

            // Antecedentes como owned type: se persisten en columnas de la misma tabla.
            builder.OwnsOne(x => x.History, h =>
            {
                h.Property(p => p.Hypertension).HasColumnName("History_Hypertension");
                h.Property(p => p.Diabetes).HasColumnName("History_Diabetes");
                h.Property(p => p.Cancer).HasColumnName("History_Cancer");
                h.Property(p => p.Pacemaker).HasColumnName("History_Pacemaker");
                h.Property(p => p.Pregnancy).HasColumnName("History_Pregnancy");
                h.Property(p => p.Surgeries).HasColumnName("History_Surgeries").HasMaxLength(2000);
                h.Property(p => p.Fractures).HasColumnName("History_Fractures").HasMaxLength(2000);
                h.Property(p => p.Medications).HasColumnName("History_Medications").HasMaxLength(2000);
                h.Property(p => p.Allergies).HasColumnName("History_Allergies").HasMaxLength(2000);
                h.Property(p => p.OtherHistory).HasColumnName("History_OtherHistory").HasMaxLength(4000);
            });
            builder.Navigation(x => x.History).IsRequired();
        }
    }
}
