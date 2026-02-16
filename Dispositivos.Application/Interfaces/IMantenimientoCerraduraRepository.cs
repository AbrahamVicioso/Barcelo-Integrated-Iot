using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface IMantenimientoCerraduraRepository
{
    Task<IEnumerable<MantenimientoCerradura>> GetAll();
    Task<MantenimientoCerradura?> GetById(int id);
    Task<IEnumerable<MantenimientoCerradura>> GetByCerraduraId(int cerraduraId);
    Task<IEnumerable<MantenimientoCerradura>> GetByDispositivoId(Guid dispositivoId);
    Task<IEnumerable<MantenimientoCerradura>> GetByEstado(string estado);
    Task<IEnumerable<MantenimientoCerradura>> GetByPersonalId(int personalId);
    Task AddAsync(MantenimientoCerradura mantenimiento, CancellationToken cancellationToken = default);
    Task UpdateAsync(MantenimientoCerradura mantenimiento, CancellationToken cancellationToken = default);
    Task DeleteAsync(MantenimientoCerradura mantenimiento, CancellationToken cancellationToken = default);
}
