using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class CarreraConfiguration : IEntityTypeConfiguration<Carrera>
{
    public void Configure(EntityTypeBuilder<Carrera> builder)
    {
        builder.ToTable("Carreras");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .HasColumnName("Nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Codigo)
            .HasColumnName("Codigo")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CodigoExterno)
            .HasColumnName("CodigoExterno");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
    }
}
