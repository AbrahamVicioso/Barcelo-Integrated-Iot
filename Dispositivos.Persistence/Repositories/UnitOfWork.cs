using Microsoft.EntityFrameworkCore;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;

namespace Dispositivos.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BarceloIoTDatabaseContext _context;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    public IDispositivoRepository Dispositivos { get; }
    public ICerradurasInteligenteRepository CerradurasInteligente { get; }
    public ICredencialesAccesoRepository CredencialesAcceso { get; }
    public IMantenimientoCerraduraRepository MantenimientoCerradura { get; }
    public IRegistrosAccesoRepository RegistrosAcceso { get; }
    public IRegistrosAuditoriumRepository RegistrosAuditorium { get; }

    public UnitOfWork(
        BarceloIoTDatabaseContext context, 
        IDispositivoRepository dispositivoRepository,
        ICerradurasInteligenteRepository cerradurasInteligenteRepository,
        ICredencialesAccesoRepository credencialesAccesoRepository,
        IMantenimientoCerraduraRepository mantenimientoCerraduraRepository,
        IRegistrosAccesoRepository registrosAccesoRepository,
        IRegistrosAuditoriumRepository registrosAuditoriumRepository)
    {
        _context = context;
        Dispositivos = dispositivoRepository;
        CerradurasInteligente = cerradurasInteligenteRepository;
        CredencialesAcceso = credencialesAccesoRepository;
        MantenimientoCerradura = mantenimientoCerraduraRepository;
        RegistrosAcceso = registrosAccesoRepository;
        RegistrosAuditorium = registrosAuditoriumRepository;
    }

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
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
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
        _context.Dispose();
        _transaction?.Dispose();
    }
}
