using Reservas.Domain.Entities;

namespace Reservas.Application.Interfaces;

public interface IHotelRepository
{
    Task<IEnumerable<Hotel>> GetAll();
    Task<Hotel?> GetById(int id);
    Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Hotel hotel, CancellationToken cancellationToken = default);
}
