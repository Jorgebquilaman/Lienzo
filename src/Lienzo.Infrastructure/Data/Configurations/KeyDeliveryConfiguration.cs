using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class KeyDeliveryConfiguration : IEntityTypeConfiguration<KeyDelivery>
{
    public void Configure(EntityTypeBuilder<KeyDelivery> builder)
    {
        builder.ToTable("entregas_llaves");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClassroomId).HasColumnName("aula_id").IsRequired();
        builder.Property(e => e.DeliveredToUserId).HasColumnName("entregado_a_usuario_id");
        builder.Property(e => e.DeliveredToName).HasColumnName("entregado_a_nombre").HasMaxLength(200).IsRequired();
        builder.Property(e => e.DeliveredById).HasColumnName("entregado_por_usuario_id").IsRequired();
        builder.Property(e => e.DeliveredAt).HasColumnName("entregado_en").IsRequired();
        builder.Property(e => e.ReturnedAt).HasColumnName("devuelto_en");
        builder.Property(e => e.Notes).HasColumnName("notas").HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("actualizado_en").IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("eliminado").IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnName("eliminado_en");

        builder.HasOne(e => e.Classroom)
            .WithMany()
            .HasForeignKey(e => e.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ClassroomId).HasDatabaseName("ix_entregas_llaves_aula");
        builder.HasIndex(e => e.DeliveredToUserId).HasDatabaseName("ix_entregas_llaves_entregado_a");
        builder.HasIndex(e => e.ReturnedAt).HasDatabaseName("ix_entregas_llaves_devuelto");
    }
}
