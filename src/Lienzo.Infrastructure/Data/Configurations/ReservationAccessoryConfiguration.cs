using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class ReservationAccessoryConfiguration : IEntityTypeConfiguration<ReservationAccessory>
{
    public void Configure(EntityTypeBuilder<ReservationAccessory> builder)
    {
        builder.ToTable("reserva_accesorios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReservationId)
            .HasColumnName("reserva_id")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Origin)
            .HasColumnName("origen")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsRequested)
            .HasColumnName("solicitado")
            .IsRequired();

        builder.Property(e => e.IsGranted)
            .HasColumnName("confirmado");

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

        builder.HasOne(e => e.Reservation)
            .WithMany(e => e.ReservationAccessories)
            .HasForeignKey(e => e.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ReservationId)
            .HasDatabaseName("ix_reserva_accesorios_reserva_id");
    }
}
