using Microsoft.EntityFrameworkCore;
using Notification.Domain.Data;

namespace Notification.Kafka.Data;

public class NotificacionDbContext : DbContext
{
    public NotificacionDbContext(DbContextOptions<NotificacionDbContext> options) : base(options)
    {
    }

    public DbSet<PreferenciaNotificacion> PreferenciasNotificacion { get; set; }
    public DbSet<NotificacionEntity> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PreferenciaNotificacion>(entity =>
        {
            entity.ToTable("PreferenciasNotificacion");
            entity.HasKey(e => e.PreferenciaId);
            entity.Property(e => e.UsuarioId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.UsuarioId).IsUnique();
        });

        modelBuilder.Entity<NotificacionEntity>(entity =>
        {
            entity.ToTable("Notificaciones");
            entity.HasKey(e => e.NotificacionId);
            entity.Property(e => e.UsuarioId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.TipoNotificacion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensaje).IsRequired();
            entity.Property(e => e.Prioridad).HasMaxLength(20);
            entity.Property(e => e.CanalEnvio).HasMaxLength(20);
            entity.Property(e => e.EstadoEnvio).HasMaxLength(20);
            entity.Property(e => e.TipoEntidadRelacionada).HasMaxLength(50);
        });
    }
}
