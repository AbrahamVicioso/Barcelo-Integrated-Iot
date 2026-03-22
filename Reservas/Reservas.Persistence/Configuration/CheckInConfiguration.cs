using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entities;

namespace Reservas.Persistence.Configuration;

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.HasKey(e => e.CheckInId);

        builder.ToTable("CheckIns");

        builder.Property(e => e.NombreCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Telefono)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.FechaCheckIn)
            .IsRequired()
            .HasDefaultValueSql("(getutcdate())");

        builder.Property(e => e.DireccionIp)
            .HasMaxLength(45);

        builder.Property(e => e.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Completado");

        builder.HasIndex(e => e.ReservaId, "IX_CheckIns_ReservaId").IsUnique();

        builder.HasIndex(e => e.Email, "IX_CheckIns_Email");

        builder.HasOne(e => e.Reserva)
            .WithMany()
            .HasForeignKey(e => e.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
