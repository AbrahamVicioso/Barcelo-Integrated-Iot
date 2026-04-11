using Microsoft.EntityFrameworkCore;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class PuestoRepository : GenericRepository<Puesto>, IPuestoRepository
{
    public PuestoRepository(BarceloIoTSystemContext context) : base(context)
    {
    }

    public async Task<Puesto?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Nombre == nombre);
    }

    public async Task<IEnumerable<Puesto>> GetActivosAsync()
    {
        return await _dbSet.Where(p => p.EstaActivo).ToListAsync();
    }
}
