using Dispositivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispositivos.Persistence.Data.Configurations;

public class EstadoDispositivoConfiguration : IEntityTypeConfiguration<EstadoDispositivo>
{
    public void Configure(EntityTypeBuilder<EstadoDispositivo> entity)
    {
        entity.ToTable("EstadosDispositivo");

        entity.HasKey(e => e.EstadoDispositivoId);

        entity.Property(e => e.EstadoDispositivoId)
            .ValueGeneratedNever();

        entity.Property(e => e.Descripcion)
            .IsRequired()
            .HasMaxLength(50);
    }
}
