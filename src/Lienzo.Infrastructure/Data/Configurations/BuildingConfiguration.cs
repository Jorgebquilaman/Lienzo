using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("edificios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Address)
            .HasColumnName("direccion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.FloorCount)
            .HasColumnName("numero_pisos")
            .IsRequired();

        builder.Property(e => e.CodigoExterno)
            .HasColumnName("codigo_externo");

        builder.Property(e => e.IsActive)
            .HasColumnName("activo")
            .IsRequired();

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

        builder.HasMany(e => e.Classrooms)
            .WithOne(e => e.Building)
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("ix_edificios_nombre");
    }
}
