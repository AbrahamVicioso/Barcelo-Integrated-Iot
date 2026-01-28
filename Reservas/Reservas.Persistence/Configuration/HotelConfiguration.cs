using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Persistence.Configuration
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.ToTable("Hoteles");

            builder.HasKey(h => h.HotelId);

            builder.Property(h => h.HotelId)
                .HasColumnName("HotelId")
                .UseIdentityColumn();

            builder.Property(h => h.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(h => h.Direccion)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.Ciudad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(h => h.Pais)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(h => h.Telefono)
                .HasMaxLength(50);

            builder.Property(h => h.Email)
                .HasMaxLength(256);

            builder.Property(h => h.EstaActivo)
                .IsRequired();

            builder.Property(h => h.FechaCreacion)
                .IsRequired()
                .HasColumnType("datetime2(7)");

            builder.Property(h => h.NumeroHabitaciones)
                .IsRequired();

            builder.Property(h => h.NumeroEstrellas)
                .HasColumnType("tinyint");
        }
    }
}
