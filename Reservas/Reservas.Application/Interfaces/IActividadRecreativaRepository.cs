using Reservas.Domain.Entites;

namespace Reservas.Application.Interfaces;

public interface IActividadRecreativaRepository : IGenericRepository<ActividadesRecreativas>
{
    Task<IEnumerable<ActividadesRecreativas>> GetActividadesActivasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ActividadesRecreativas>> GetActividadesByCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActividadesRecreativas>> GetActividadesByHotelIdAsync(int hotelId, CancellationToken cancellationToken = default);
}
