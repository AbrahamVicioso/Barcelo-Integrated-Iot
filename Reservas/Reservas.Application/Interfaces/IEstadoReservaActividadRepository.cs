using Reservas.Domain.Entites;

namespace Reservas.Application.Interfaces;

public interface IEstadoReservaActividadRepository
{
    Task<IEnumerable<EstadoReservaActividad>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EstadoReservaActividad?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(EstadoReservaActividad entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(EstadoReservaActividad entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(EstadoReservaActividad entity, CancellationToken cancellationToken = default);
}