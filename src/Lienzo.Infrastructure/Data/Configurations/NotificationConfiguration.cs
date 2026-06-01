using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notificaciones");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Body)
            .HasColumnName("cuerpo")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.IsRead)
            .HasColumnName("leido")
            .IsRequired();

        builder.Property(e => e.RelatedEntityId)
            .HasColumnName("entidad_relacionada_id");

        builder.Property(e => e.RelatedEntityType)
            .HasColumnName("tipo_entidad_relacionada")
            .HasMaxLength(100);

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

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_notificaciones_usuario_id");

        builder.HasIndex(e => e.IsRead)
            .HasDatabaseName("ix_notificaciones_leido");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_notificaciones_creado_en");
    }
}
