using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Persistence.Configuration
{
    public class ActividadesRecreativaConfiguration : IEntityTypeConfiguration<ActividadesRecreativa>
    {
        public void Configure(EntityTypeBuilder<ActividadesRecreativa> builder)
        {
            builder.HasKey(e => e.ActividadId);

            builder.Property(e => e.Categoria)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(e => e.Descripcion).HasMaxLength(2000);
            builder.Property(e => e.EstaActiva).HasDefaultValue(true);
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("(getutcdate())");
            builder.Property(e => e.ImagenUrl)
                .HasMaxLength(500)
                .HasColumnName("ImagenURL");
            builder.Property(e => e.NombreActividad)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(e => e.PrecioPorPersona).HasColumnType("decimal(10, 2)");
            builder.Property(e => e.Ubicacion)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}
