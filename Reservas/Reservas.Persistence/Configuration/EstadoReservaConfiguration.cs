using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entites;

namespace Reservas.Persistence.Configuration;

public class EstadoReservaConfiguration : IEntityTypeConfiguration<EstadoReserva>
{
    public void Configure(EntityTypeBuilder<EstadoReserva> builder)
    {
        builder.ToTable("EstadosReserva");

        builder.HasKey(e => e.EstadoReservaId);

        builder.Property(e => e.EstadoReservaId)
            .ValueGeneratedNever();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Descripcion)
            .HasMaxLength(200);

    }
}
