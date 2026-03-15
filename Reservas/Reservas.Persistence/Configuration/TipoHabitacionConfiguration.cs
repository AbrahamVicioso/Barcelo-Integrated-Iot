using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entities;

namespace Reservas.Persistence.Configuration;

public class TipoHabitacionConfiguration : IEntityTypeConfiguration<TipoHabitacion>
{
    public void Configure(EntityTypeBuilder<TipoHabitacion> builder)
    {
        builder.ToTable("TiposHabitacion");

        builder.HasKey(t => t.TipoHabitacionId);

        builder.Property(t => t.TipoHabitacionId)
            .ValueGeneratedNever();

        builder.Property(t => t.Nombre)
            .HasMaxLength(50)
            .IsRequired();
    }
}
