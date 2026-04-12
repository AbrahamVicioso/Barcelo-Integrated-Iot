using Microsoft.EntityFrameworkCore;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class TipoDocumentoRepository : GenericRepository<TipoDocumento>, ITipoDocumentoRepository
{
    public TipoDocumentoRepository(BarceloIoTSystemContext context) : base(context)
    {
    }

    public async Task<TipoDocumento?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Nombre == nombre);
    }

    public async Task<IEnumerable<TipoDocumento>> GetActivosAsync()
    {
        return await _dbSet.Where(t => t.EstaActivo).ToListAsync();
    }
}
