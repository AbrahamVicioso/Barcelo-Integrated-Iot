using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entites;

namespace Reservas.Persistence.Configuration
{
    public class ReservasActividadesConfiguration : IEntityTypeConfiguration<ReservasActividades>
    {

        public void Configure(EntityTypeBuilder<ReservasActividades> builder)
        {
            builder.ToTable("ReservasActividades");
            builder.HasKey(e => e.ReservaActividadId);

            builder.HasIndex(e => new { e.FechaReserva, e.HoraReserva }, "IX_ReservasActividades_Fecha");

            builder.HasIndex(e => e.HuespedId, "IX_ReservasActividades_HuespedId");

            builder.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Confirmada");
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("(getutcdate())");
            builder.Property(e => e.FechaReserva).HasColumnType("date");
            builder.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");
            builder.Property(e => e.NotasEspeciales).HasMaxLength(500);
            builder.Property(e => e.NumeroPersonas).HasDefaultValue(1);

            builder.HasOne(d => d.Actividad).WithMany(p => p.ReservasActividades)
                .HasForeignKey(d => d.ActividadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReservasActividades_Actividades");
        }
    }
}
