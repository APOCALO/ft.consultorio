using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsMedicalRecords.Domain.Sessions;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Configurations
{
    public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

            builder.Property(x => x.MedicalRecordId).IsRequired();
            builder.HasIndex(x => x.MedicalRecordId);

            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.PainScale).IsRequired();
            builder.Property(x => x.Evolution).HasMaxLength(4000);
            builder.Property(x => x.TreatmentPerformed).HasMaxLength(4000);
            builder.Property(x => x.Recommendations).HasMaxLength(4000);
            builder.Property(x => x.Price).HasPrecision(18, 2);
            builder.Property(x => x.Paid).IsRequired();
            builder.Property(x => x.PaidAt);
        }
    }
}
