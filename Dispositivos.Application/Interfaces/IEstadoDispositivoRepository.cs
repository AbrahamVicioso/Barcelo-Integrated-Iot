using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface IEstadoDispositivoRepository
{
    Task<IEnumerable<EstadoDispositivo>> GetAll();
    Task<EstadoDispositivo?> GetById(int id);
    Task AddAsync(EstadoDispositivo estadoDispositivo, CancellationToken cancellationToken = default);
    Task UpdateAsync(EstadoDispositivo estadoDispositivo, CancellationToken cancellationToken = default);
    Task DeleteAsync(EstadoDispositivo estadoDispositivo, CancellationToken cancellationToken = default);
}
