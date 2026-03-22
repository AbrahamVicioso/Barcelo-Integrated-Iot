using Reservas.Domain.Entites;

namespace Reservas.Application.Interfaces;

public interface IEstadoReservaRepository
{
    Task<IEnumerable<EstadoReserva>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EstadoReserva?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
