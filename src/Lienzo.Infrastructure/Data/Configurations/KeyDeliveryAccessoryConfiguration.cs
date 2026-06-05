using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class KeyDeliveryAccessoryConfiguration : IEntityTypeConfiguration<KeyDeliveryAccessory>
{
    public void Configure(EntityTypeBuilder<KeyDeliveryAccessory> builder)
    {
        builder.ToTable("entrega_accesorios");

        builder.HasKey(e => new { e.KeyDeliveryId, e.AccessoryId });

        builder.Property(e => e.KeyDeliveryId)
            .HasColumnName("entrega_llave_id")
            .IsRequired();

        builder.Property(e => e.AccessoryId)
            .HasColumnName("accesorio_id")
            .IsRequired();

        builder.HasOne(e => e.KeyDelivery)
            .WithMany(k => k.Accessories)
            .HasForeignKey(e => e.KeyDeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Accessory)
            .WithMany()
            .HasForeignKey(e => e.AccessoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
