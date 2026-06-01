using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ClassroomSurveyConfiguration : IEntityTypeConfiguration<ClassroomSurvey>
{
    public void Configure(EntityTypeBuilder<ClassroomSurvey> builder)
    {
        builder.ToTable("encuestas_aula");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReservationId).HasColumnName("reserva_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("usuario_id").IsRequired();
        builder.Property(e => e.ConditionRating).HasColumnName("calificacion_condicion").HasPrecision(2, 1).IsRequired();
        builder.Property(e => e.EquipmentRating).HasColumnName("calificacion_equipamiento").HasPrecision(2, 1).IsRequired();
        builder.Property(e => e.CleanlinessRating).HasColumnName("calificacion_limpieza").HasPrecision(2, 1).IsRequired();
        builder.Property(e => e.OverallRating).HasColumnName("calificacion_general").HasPrecision(2, 1).IsRequired();
        builder.Property(e => e.Comment).HasColumnName("comentario").HasMaxLength(1000);
        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("actualizado_en").IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("eliminado").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("eliminado_en");

        builder.HasOne(e => e.Reservation)
            .WithMany()
            .HasForeignKey(e => e.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReservationId).IsUnique().HasDatabaseName("ix_encuestas_aula_reserva");
        builder.HasIndex(e => e.UserId).HasDatabaseName("ix_encuestas_aula_usuario");
    }
}
