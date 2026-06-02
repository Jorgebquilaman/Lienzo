using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ClaseConfiguration : IEntityTypeConfiguration<Clase>
{
    public void Configure(EntityTypeBuilder<Clase> builder)
    {
        builder.ToTable("clases_asistencia");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Fecha).IsRequired();
        builder.Property(e => e.HoraInicio).IsRequired();
        builder.Property(e => e.HoraFin).IsRequired();
        builder.Property(e => e.SgaComisionId).IsRequired();
        builder.Property(e => e.SgaClaseId);
        builder.Property(e => e.CheckedInAt).IsRequired();
        builder.Property(e => e.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(e => e.Reservation)
            .WithMany()
            .HasForeignKey(e => e.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Actividad)
            .WithMany()
            .HasForeignKey(e => e.ActividadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Classroom)
            .WithMany()
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Asistencias)
            .WithOne(a => a.Clase)
            .HasForeignKey(a => a.ClaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
