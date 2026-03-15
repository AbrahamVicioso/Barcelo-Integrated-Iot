using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entities;

namespace Reservas.Persistence.Configuration;

public class EstadoHabitacionConfiguration : IEntityTypeConfiguration<EstadoHabitacion>
{
    public void Configure(EntityTypeBuilder<EstadoHabitacion> builder)
    {
        builder.ToTable("EstadosHabitacion");

        builder.HasKey(e => e.EstadoHabitacionId);

        builder.Property(e => e.EstadoHabitacionId)
            .ValueGeneratedNever();

        builder.Property(e => e.Descripcion)
            .IsRequired()
            .HasMaxLength(50);
    }
}
