namespace Dispositivos.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDispositivoRepository Dispositivos { get; }
    ICerradurasInteligenteRepository CerradurasInteligente { get; }
    ICredencialesAccesoRepository CredencialesAcceso { get; }
    IMantenimientoCerraduraRepository MantenimientoCerradura { get; }
    IRegistrosAccesoRepository RegistrosAcceso { get; }
    IRegistrosAuditoriumRepository RegistrosAuditorium { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
