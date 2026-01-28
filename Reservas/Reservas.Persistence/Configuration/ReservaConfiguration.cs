using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entites;

namespace Reservas.Persistence.Configuration
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> builder)
        {
            builder.ToTable(tb =>
            {
                tb.HasTrigger("TRG_ActualizarEstadoHabitacion");
                tb.HasTrigger("TRG_Audit_Reservas");
                tb.HasTrigger("TRG_DesactivarCredencialesCheckout");
            });

            builder.HasIndex(e => new { e.Estado, e.FechaCheckIn, e.FechaCheckOut }, "IX_Reservas_Estado_Fechas");

            builder.HasIndex(e => e.HabitacionId, "IX_Reservas_HabitacionId");

            builder.HasIndex(e => e.HuespedId, "IX_Reservas_HuespedId");

            builder.HasIndex(e => e.NumeroReserva, "IX_Reservas_NumeroReserva");

            builder.HasIndex(e => e.NumeroReserva, "UQ_Reservas_NumeroReserva").IsUnique();

            builder.Property(e => e.CreadoPor).HasMaxLength(450);
            builder.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Pendiente");
            builder.Property(e => e.FechaCreacion).HasDefaultValueSql("(getutcdate())");
            builder.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            builder.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");
            builder.Property(e => e.NumeroHuespedes).HasDefaultValue(1);
            builder.Property(e => e.NumeroReserva)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(e => e.Observaciones).HasMaxLength(1000);
        }
    }
}
