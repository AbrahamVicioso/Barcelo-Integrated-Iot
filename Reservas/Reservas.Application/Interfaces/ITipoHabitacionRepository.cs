using Reservas.Domain.Entities;

namespace Reservas.Application.Interfaces;

public interface ITipoHabitacionRepository
{
    Task<IEnumerable<TipoHabitacion>> GetAll();
    Task<TipoHabitacion?> GetById(int id);
    Task AddAsync(TipoHabitacion tipoHabitacion, CancellationToken cancellationToken = default);
    Task UpdateAsync(TipoHabitacion tipoHabitacion, CancellationToken cancellationToken = default);
    Task DeleteAsync(TipoHabitacion tipoHabitacion, CancellationToken cancellationToken = default);
}
