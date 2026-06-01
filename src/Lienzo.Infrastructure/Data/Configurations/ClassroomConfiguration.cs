using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.ToTable("aulas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BuildingId)
            .HasColumnName("edificio_id")
            .IsRequired();

        builder.Property(e => e.Floor)
            .HasColumnName("piso")
            .IsRequired();

        builder.Property(e => e.Capacity)
            .HasColumnName("capacidad")
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Features)
            .HasColumnName("caracteristicas")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("activo")
            .IsRequired();

        builder.Property(e => e.ImageUrl)
            .HasColumnName("url_imagen")
            .HasMaxLength(500);

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

        builder.HasOne(e => e.Building)
            .WithMany(e => e.Classrooms)
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Reservations)
            .WithOne(e => e.Classroom)
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("ix_aulas_nombre");

        builder.HasIndex(e => e.BuildingId)
            .HasDatabaseName("ix_aulas_edificio_id");
    }
}
