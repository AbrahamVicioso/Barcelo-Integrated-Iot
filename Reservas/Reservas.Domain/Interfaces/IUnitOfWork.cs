namespace Reservas.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IReservaRepository Reservas { get; }
    IActividadRecreativaRepository ActividadesRecreativas { get; }
    IReservaActividadRepository ReservasActividades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
