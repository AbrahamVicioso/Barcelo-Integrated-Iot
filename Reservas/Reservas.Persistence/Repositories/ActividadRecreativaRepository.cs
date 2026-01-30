using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entites;
using Reservas.Persistence.Data;
using Reservas.Application.Interfaces;

namespace Reservas.Persistence.Repositories;

public class ActividadRecreativaRepository : GenericRepository<ActividadesRecreativas>, IActividadRecreativaRepository
{
    public ActividadRecreativaRepository(BarceloReservasContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActividadesRecreativas>> GetActividadesActivasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EstaActiva)
            .OrderBy(a => a.NombreActividad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActividadesRecreativas>> GetActividadesByCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Categoria == categoria && a.EstaActiva)
            .OrderBy(a => a.NombreActividad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActividadesRecreativas>> GetActividadesByHotelIdAsync(int hotelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.HotelId == hotelId && a.EstaActiva)
            .OrderBy(a => a.NombreActividad)
            .ToListAsync(cancellationToken);
    }
}
