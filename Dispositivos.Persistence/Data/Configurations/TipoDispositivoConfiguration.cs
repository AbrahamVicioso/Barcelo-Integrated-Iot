using Dispositivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispositivos.Persistence.Data.Configurations;

public class TipoDispositivoConfiguration : IEntityTypeConfiguration<TipoDispositivo>
{
    public void Configure(EntityTypeBuilder<TipoDispositivo> entity)
    {
        entity.ToTable("TiposDispositivo");

        entity.HasKey(e => e.TipoDispositivoId);

        entity.Property(e => e.TipoDispositivoId)
            .ValueGeneratedNever();

        entity.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(50);
    }
}
