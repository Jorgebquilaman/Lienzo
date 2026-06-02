using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ActividadConfiguration : IEntityTypeConfiguration<Actividad>
{
    public void Configure(EntityTypeBuilder<Actividad> builder)
    {
        builder.ToTable("Actividades");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasColumnName("Nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.CodigoMateria)
            .HasColumnName("CodigoMateria")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.PeriodoId)
            .HasColumnName("PeriodoId")
            .IsRequired();

        builder.Property(e => e.CarreraId)
            .HasColumnName("CarreraId")
            .IsRequired();

        builder.Property(e => e.CodigoExterno)
            .HasColumnName("CodigoExterno");

        builder.Property(e => e.ComisionNombre)
            .HasColumnName("ComisionNombre")
            .HasMaxLength(200);

        builder.Property(e => e.AulaId)
            .HasColumnName("AulaId");

        builder.Property(e => e.DiaSemana)
            .HasColumnName("DiaSemana")
            .HasMaxLength(20);

        builder.Property(e => e.HoraInicio)
            .HasColumnName("HoraInicio");

        builder.Property(e => e.HoraFin)
            .HasColumnName("HoraFin");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();

        builder.HasMany(e => e.Docentes)
            .WithOne(e => e.Actividad)
            .HasForeignKey(e => e.ActividadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
