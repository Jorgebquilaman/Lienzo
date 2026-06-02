using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class AsistenciaAlumnoConfiguration : IEntityTypeConfiguration<AsistenciaAlumno>
{
    public void Configure(EntityTypeBuilder<AsistenciaAlumno> builder)
    {
        builder.ToTable("asistencias_alumnos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SgaAlumnoId).IsRequired();
        builder.Property(e => e.SgaPersonaId).IsRequired();
        builder.Property(e => e.AlumnoNombre).HasMaxLength(200).IsRequired();
        builder.Property(e => e.AlumnoDocumento).HasMaxLength(50);
        builder.Property(e => e.SgaAsistenciaId);

        builder.HasOne(e => e.Clase)
            .WithMany(c => c.Asistencias)
            .HasForeignKey(e => e.ClaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
