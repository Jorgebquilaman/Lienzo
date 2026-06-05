using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class AccessoryConfiguration : IEntityTypeConfiguration<Accessory>
{
    public void Configure(EntityTypeBuilder<Accessory> builder)
    {
        builder.ToTable("accesorios_bedelia");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("descripcion")
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .HasColumnName("activo")
            .IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("actualizado_en").IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("eliminado").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("eliminado_en");
    }
}
