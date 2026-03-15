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

        builder.Property(h => h.TipoHabitacionId)
            .IsRequired();

        builder.Property(h => h.Piso)
            .IsRequired();

        builder.Property(h => h.CapacidadMaxima)
            .HasDefaultValue(2)
            .IsRequired();

        builder.Property(h => h.PrecioPorNoche)
            .HasColumnType("decimal(10, 2)")
            .IsRequired();

        builder.Property(h => h.EstadoHabitacionId)
            .IsRequired()
            .HasDefaultValue(1);

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

        // Foreign key to EstadoHabitacion
        builder.HasOne(h => h.EstadoHabitacion)
            .WithMany(e => e.Habitaciones)
            .HasForeignKey(h => h.EstadoHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key to TipoHabitacion
        builder.HasOne(h => h.TipoHabitacion)
            .WithMany(t => t.Habitaciones)
            .HasForeignKey(h => h.TipoHabitacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(h => h.EstadoHabitacionId);
        builder.HasIndex(h => h.HotelId);
    }
}
