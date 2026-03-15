using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface ITipoDispositivoRepository
{
    Task<IEnumerable<TipoDispositivo>> GetAll();
    Task<TipoDispositivo?> GetById(int id);
    Task AddAsync(TipoDispositivo tipo, CancellationToken cancellationToken = default);
    Task UpdateAsync(TipoDispositivo tipo, CancellationToken cancellationToken = default);
    Task DeleteAsync(TipoDispositivo tipo, CancellationToken cancellationToken = default);
}
