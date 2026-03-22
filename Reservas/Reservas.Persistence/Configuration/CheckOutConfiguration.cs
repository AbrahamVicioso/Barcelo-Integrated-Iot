using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.Domain.Entities;

namespace Reservas.Persistence.Configuration;

public class CheckOutConfiguration : IEntityTypeConfiguration<CheckOut>
{
    public void Configure(EntityTypeBuilder<CheckOut> builder)
    {
        builder.HasKey(e => e.CheckOutId);

        builder.ToTable("CheckOuts");

        builder.Property(e => e.FechaCheckOut)
            .IsRequired()
            .HasDefaultValueSql("(getutcdate())");

        builder.HasIndex(e => e.ReservaId, "IX_CheckOuts_ReservaId").IsUnique();

        builder.HasOne(e => e.Reserva)
            .WithMany()
            .HasForeignKey(e => e.ReservaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
