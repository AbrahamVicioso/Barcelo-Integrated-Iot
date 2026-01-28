using Reservas.Domain.Entities;

namespace Reservas.Application.Interfaces;

public interface IHabitacionRepository
{
    Task<IEnumerable<Habitacion>> GetAll();
    Task<Habitacion?> GetById(int id);
    Task<IEnumerable<Habitacion>> GetByHotelId(int hotelId);
    Task AddAsync(Habitacion habitacion, CancellationToken cancellationToken = default);
    Task UpdateAsync(Habitacion habitacion, CancellationToken cancellationToken = default);
    Task DeleteAsync(Habitacion habitacion, CancellationToken cancellationToken = default);
}
