using Reservas.Domain.Entities;

namespace Reservas.Application.Interfaces;

public interface IEstadoHabitacionRepository
{
    Task<IEnumerable<EstadoHabitacion>> GetAll();
    Task<EstadoHabitacion?> GetById(int id);
    Task AddAsync(EstadoHabitacion estadoHabitacion, CancellationToken cancellationToken = default);
    Task UpdateAsync(EstadoHabitacion estadoHabitacion, CancellationToken cancellationToken = default);
    Task DeleteAsync(EstadoHabitacion estadoHabitacion, CancellationToken cancellationToken = default);
}
