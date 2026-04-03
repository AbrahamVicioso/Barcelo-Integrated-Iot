using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entites;

namespace Reservas.Persistence.Configuration;

public class ReservaHuespedConfiguration : IEntityTypeConfiguration<ReservaHuesped>
{
    public void Configure(EntityTypeBuilder<ReservaHuesped> builder)
    {
        builder.HasKey(e => new { e.ReservaId, e.HuespedId });

        builder.Property(e => e.PuedeCrearActividadesRecreativas).HasDefaultValue(false);
        builder.Property(e => e.PuedeDesbloquearCerradura).HasDefaultValue(false);
        builder.Property(e => e.FechaAgregado).HasDefaultValueSql("(getutcdate())");

        builder.HasIndex(e => e.HuespedId, "IX_ReservaHuespedes_HuespedId");

        builder.HasOne(e => e.Reserva)
            .WithMany(r => r.ReservaHuespedes)
            .HasForeignKey(e => e.ReservaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ReservaHuespedes_Reservas");
    }
}
