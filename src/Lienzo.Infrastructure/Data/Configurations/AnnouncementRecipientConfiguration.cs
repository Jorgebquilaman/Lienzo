using Lienzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lienzo.Infrastructure.Data.Configurations;

public class AnnouncementRecipientConfiguration : IEntityTypeConfiguration<AnnouncementRecipient>
{
    public void Configure(EntityTypeBuilder<AnnouncementRecipient> builder)
    {
        builder.ToTable("comunicado_destinatarios");

        builder.HasKey(e => new { e.AnnouncementId, e.StudentId });

        builder.Property(e => e.AnnouncementId)
            .HasColumnName("comunicado_id")
            .IsRequired();

        builder.Property(e => e.StudentId)
            .HasColumnName("estudiante_id")
            .IsRequired();

        builder.Property(e => e.ReadAt)
            .HasColumnName("leido_en");

        builder.HasOne(e => e.Announcement)
            .WithMany(e => e.Recipients)
            .HasForeignKey(e => e.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.StudentId)
            .HasDatabaseName("ix_comunicado_destinatarios_estudiante_id");
    }
}
