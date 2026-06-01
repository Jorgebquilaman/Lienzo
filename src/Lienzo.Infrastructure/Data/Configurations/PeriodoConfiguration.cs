using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class PeriodoConfiguration : IEntityTypeConfiguration<Periodo>
{
    public void Configure(EntityTypeBuilder<Periodo> builder)
    {
        builder.ToTable("periodos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(e => e.FechaFin)
            .HasColumnName("fecha_fin")
            .IsRequired();

        builder.Property(e => e.Anio)
            .HasColumnName("anio")
            .IsRequired();

        builder.Property(e => e.CodigoExterno)
            .HasColumnName("codigo_externo");

        builder.Property(e => e.TipoPeriodoId)
            .HasColumnName("tipo_periodo_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("creado_en")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("actualizado_en")
            .IsRequired();

        builder.HasOne(e => e.TipoPeriodo)
            .WithMany()
            .HasForeignKey(e => e.TipoPeriodoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
