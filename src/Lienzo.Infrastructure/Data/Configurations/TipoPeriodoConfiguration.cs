using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class TipoPeriodoConfiguration : IEntityTypeConfiguration<TipoPeriodo>
{
    public void Configure(EntityTypeBuilder<TipoPeriodo> builder)
    {
        builder.ToTable("tipos_periodo");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.CodigoExterno)
            .HasColumnName("codigo_externo");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("creado_en")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("actualizado_en")
            .IsRequired();
    }
}
