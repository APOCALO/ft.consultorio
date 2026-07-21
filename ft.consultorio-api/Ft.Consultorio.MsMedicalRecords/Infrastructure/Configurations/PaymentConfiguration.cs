using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ft.Consultorio.MsMedicalRecords.Domain.Payments;

namespace Ft.Consultorio.MsMedicalRecords.Infrastructure.Configurations
{
    public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

            builder.Property(x => x.PatientId).IsRequired();
            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.SessionId);

            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Method).HasConversion<int>().IsRequired();
            builder.Property(x => x.Reference).HasMaxLength(200);
            builder.Property(x => x.PaidAt).IsRequired();
        }
    }
}
