using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ProcessedEmailConfiguration : IEntityTypeConfiguration<ProcessedEmail>
{
    public void Configure(EntityTypeBuilder<ProcessedEmail> builder)
    {
        builder.ToTable("processed_emails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmailUid)
            .HasColumnName("email_uid")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ReservationId)
            .HasColumnName("reserva_id")
            .IsRequired();

        builder.Property(e => e.ProcessedByUserId)
            .HasColumnName("procesado_por")
            .IsRequired();

        builder.Property(e => e.ProcessedAt)
            .HasColumnName("procesado_en")
            .IsRequired();

        builder.HasIndex(e => e.EmailUid)
            .IsUnique()
            .HasDatabaseName("ix_processed_emails_email_uid");

        builder.HasIndex(e => e.ReservationId)
            .HasDatabaseName("ix_processed_emails_reserva_id");
    }
}
