using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ReservationReminderConfiguration : IEntityTypeConfiguration<ReservationReminder>
{
    public void Configure(EntityTypeBuilder<ReservationReminder> builder)
    {
        builder.ToTable("recordatorios_reserva");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReservationId).HasColumnName("reserva_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("usuario_id").IsRequired();
        builder.Property(e => e.ReminderType).HasColumnName("tipo_recordatorio").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.SentAt).HasColumnName("enviado_en").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("actualizado_en").IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("eliminado").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("eliminado_en");

        builder.HasIndex(e => new { e.ReservationId, e.ReminderType }).HasDatabaseName("ix_recordatorios_reserva_tipo");
        builder.HasIndex(e => e.SentAt).HasDatabaseName("ix_recordatorios_reserva_enviado_en");
    }
}
