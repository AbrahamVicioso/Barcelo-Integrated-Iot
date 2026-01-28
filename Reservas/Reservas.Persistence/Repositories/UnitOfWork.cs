using Microsoft.EntityFrameworkCore.Storage;
using Reservas.Application.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BarceloReservasContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(BarceloReservasContext context)
    {
        _context = context;
        Reservas = new ReservaRepository(_context);
        ActividadesRecreativas = new ActividadRecreativaRepository(_context);
        ReservasActividades = new ReservaActividadRepository(_context);
        Hoteles = new HotelRepository(_context);
        Habitaciones = new HabitacionRepository(_context);
    }

    public IReservaRepository Reservas { get; private set; }
    public IActividadRecreativaRepository ActividadesRecreativas { get; private set; }
    public IReservaActividadRepository ReservasActividades { get; private set; }
    public IHotelRepository Hoteles { get; private set; }
    public IHabitacionRepository Habitaciones { get; private set; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
