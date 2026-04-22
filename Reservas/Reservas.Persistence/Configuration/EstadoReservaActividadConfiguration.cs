using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entites;

namespace Reservas.Persistence.Configuration;

public class EstadoReservaActividadConfiguration : IEntityTypeConfiguration<EstadoReservaActividad>
{
    public void Configure(EntityTypeBuilder<EstadoReservaActividad> builder)
    {
        builder.ToTable("EstadosReservaActividad");

        builder.HasKey(e => e.EstadoReservaActividadId);

        builder.Property(e => e.EstadoReservaActividadId)
            .ValueGeneratedNever();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(200);
    }
}