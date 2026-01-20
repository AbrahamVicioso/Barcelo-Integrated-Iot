using Reservas.Domain.Entites;

namespace Reservas.Domain.Interfaces;

public interface IActividadRecreativaRepository : IGenericRepository<ActividadesRecreativa>
{
    Task<IEnumerable<ActividadesRecreativa>> GetActividadesActivasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ActividadesRecreativa>> GetActividadesByCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActividadesRecreativa>> GetActividadesByHotelIdAsync(int hotelId, CancellationToken cancellationToken = default);
}
