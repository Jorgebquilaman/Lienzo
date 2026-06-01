using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClassroomId)
            .HasColumnName("aula_id")
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("descripcion")
            .HasMaxLength(1000);

        builder.Property(e => e.Date)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(e => e.StartTime)
            .HasColumnName("hora_inicio")
            .IsRequired();

        builder.Property(e => e.EndTime)
            .HasColumnName("hora_fin")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ApprovedById)
            .HasColumnName("aprobado_por");

        builder.Property(e => e.RecurringGroupId)
            .HasColumnName("grupo_recurrente_id");

        builder.Property(e => e.RecurrenceRule)
            .HasColumnName("regla_recurrencia")
            .HasMaxLength(500);

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

        builder.HasOne(e => e.Classroom)
            .WithMany(e => e.Reservations)
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ClassroomId)
            .HasDatabaseName("ix_reservas_aula_id");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_reservas_usuario_id");

        builder.HasIndex(e => e.Date)
            .HasDatabaseName("ix_reservas_fecha");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("ix_reservas_estado");
    }
}
