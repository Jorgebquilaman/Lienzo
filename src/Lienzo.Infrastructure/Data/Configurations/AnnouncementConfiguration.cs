using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("comunicados");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TeacherId)
            .HasColumnName("profesor_id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Body)
            .HasColumnName("cuerpo")
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TargetAudience)
            .HasColumnName("audiencia_destino")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.RelatedReservationId)
            .HasColumnName("reserva_relacionada_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("creado_en")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("actualizado_en")
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .HasColumnName("eliminado")
            .IsRequired();

        builder.Property(e => e.DeletedAt)
            .HasColumnName("eliminado_en");

        builder.HasMany(e => e.Recipients)
            .WithOne(e => e.Announcement)
            .HasForeignKey(e => e.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RelatedReservation)
            .WithMany()
            .HasForeignKey(e => e.RelatedReservationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.TeacherId)
            .HasDatabaseName("ix_comunicados_profesor_id");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_comunicados_creado_en");
    }
}
