using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entites;
using Reservas.Domain.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class ActividadRecreativaRepository : GenericRepository<ActividadesRecreativa>, IActividadRecreativaRepository
{
    public ActividadRecreativaRepository(BarceloReservasContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActividadesRecreativa>> GetActividadesActivasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.EstaActiva)
            .OrderBy(a => a.NombreActividad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActividadesRecreativa>> GetActividadesByCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Categoria == categoria && a.EstaActiva)
            .OrderBy(a => a.NombreActividad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActividadesRecreativa>> GetActividadesByHotelIdAsync(int hotelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.HotelId == hotelId && a.EstaActiva)
            .OrderBy(a => a.NombreActividad)
            .ToListAsync(cancellationToken);
    }
}
