namespace Reservas.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IReservaRepository Reservas { get; }
    IActividadRecreativaRepository ActividadesRecreativas { get; }
    IReservaActividadRepository ReservasActividades { get; }
    IHotelRepository Hoteles { get; }
    IHabitacionRepository Habitaciones { get; }
    IEstadoHabitacionRepository EstadosHabitacion { get; }
    ITipoHabitacionRepository TiposHabitacion { get; }
    ICheckInRepository CheckIns { get; }
    IEstadoReservaRepository EstadosReserva { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
