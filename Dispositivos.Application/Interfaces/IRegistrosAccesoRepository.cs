using Dispositivos.Domain.Entities;

namespace Dispositivos.Application.Interfaces;

public interface IRegistrosAccesoRepository
{
    Task<IEnumerable<RegistrosAcceso>> GetAllAsync();
    Task<RegistrosAcceso?> GetByIdAsync(int id);
    Task<IEnumerable<RegistrosAcceso>> GetByCerraduraIdAsync(int cerraduraId);
    Task<IEnumerable<RegistrosAcceso>> GetByUsuarioIdAsync(string usuarioId);
    Task<IEnumerable<RegistrosAcceso>> GetByFueExitosoAsync(bool fueExitoso);
    Task<RegistrosAcceso> AddAsync(RegistrosAcceso registro, CancellationToken cancellationToken = default);
    Task UpdateAsync(RegistrosAcceso registro, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
