using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ClassroomAccessoryConfiguration : IEntityTypeConfiguration<ClassroomAccessory>
{
    public void Configure(EntityTypeBuilder<ClassroomAccessory> builder)
    {
        builder.ToTable("aulas_accesorios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClassroomId)
            .HasColumnName("aula_id")
            .IsRequired();

        builder.Property(e => e.AccessoryId)
            .HasColumnName("accesorio_id")
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

        builder.HasOne(e => e.Classroom)
            .WithMany(e => e.ClassroomAccessories)
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Accessory)
            .WithMany()
            .HasForeignKey(e => e.AccessoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ClassroomId, e.AccessoryId })
            .HasDatabaseName("ix_aulas_accesorios_aula_accesorio")
            .IsUnique()
            .HasFilter("\"eliminado\" = false");
    }
}
