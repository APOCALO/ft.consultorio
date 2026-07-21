using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ft.Consultorio.BuildingBlocks.Persistence.Outbox
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Type)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(m => m.Content)
                .IsRequired();

            builder.Property(m => m.Error)
                .HasMaxLength(1024);

            // El barrido del procesador filtra por pendientes en orden cronológico.
            // Sintaxis de filtro de PostgreSQL (identificadores entre comillas dobles).
            builder.HasIndex(m => new { m.ProcessedOnUtc, m.OccurredOnUtc })
                .HasFilter("\"ProcessedOnUtc\" IS NULL");
        }
    }
}
