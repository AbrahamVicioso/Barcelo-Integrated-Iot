using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entities;

namespace Reservas.Persistence.Configuration;

public class HabitacionConfiguration : IEntityTypeConfiguration<Habitacion>
{
    public void Configure(EntityTypeBuilder<Habitacion> builder)
    {
        builder.ToTable("Habitaciones");

        builder.HasKey(h => h.HabitacionId);

        builder.Property(h => h.HabitacionId)
            .ValueGeneratedOnAdd();

        builder.Property(h => h.HotelId)
            .IsRequired();

        builder.Property(h => h.NumeroHabitacion)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(h => h.TipoHabitacion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.Piso)
            .IsRequired();

        builder.Property(h => h.CapacidadMaxima)
            .HasDefaultValue(2)
            .IsRequired();

        builder.Property(h => h.PrecioPorNoche)
            .HasColumnType("decimal(10, 2)")
            .IsRequired();

        builder.Property(h => h.Estado)
            .HasMaxLength(20)
            .HasDefaultValue("Disponible")
            .IsRequired();

        builder.Property(h => h.EstaDisponible)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(h => h.Descripcion)
            .HasMaxLength(1000);

        builder.Property(h => h.FechaCreacion)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(h => h.FechaActualizacion);

        // Unique constraint for Hotel + NumeroHabitacion
        builder.HasIndex(h => new { h.HotelId, h.NumeroHabitacion })
            .IsUnique();

        // Foreign key to Hotel
        builder.HasOne(h => h.Hotel)
            .WithMany()
            .HasForeignKey(h => h.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(h => h.Estado);
        builder.HasIndex(h => h.HotelId);
    }
}
