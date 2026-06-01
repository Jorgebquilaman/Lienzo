using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class MaintenanceBlockConfiguration : IEntityTypeConfiguration<MaintenanceBlock>
{
    public void Configure(EntityTypeBuilder<MaintenanceBlock> builder)
    {
        builder.ToTable("bloques_mantenimiento");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClassroomId).HasColumnName("aula_id").IsRequired();
        builder.Property(e => e.StartTime).HasColumnName("inicio").IsRequired();
        builder.Property(e => e.EndTime).HasColumnName("fin").IsRequired();
        builder.Property(e => e.Reason).HasColumnName("motivo").HasMaxLength(500).IsRequired();
        builder.Property(e => e.CreatedBy).HasColumnName("creado_por").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("activo").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("actualizado_en").IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("eliminado").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("eliminado_en");

        builder.HasOne(e => e.Classroom)
            .WithMany()
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ClassroomId).HasDatabaseName("ix_bloques_mantenimiento_aula");
        builder.HasIndex(e => e.IsActive).HasDatabaseName("ix_bloques_mantenimiento_activo");
    }
}
