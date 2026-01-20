using Microsoft.EntityFrameworkCore;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class HuespedeRepository : GenericRepository<Huespede>, IHuespedeRepository
{
    public HuespedeRepository(BarceloIoTSystemContext context) : base(context)
    {
    }

    public async Task<Huespede?> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _dbSet.FirstOrDefaultAsync(h => h.UsuarioId == usuarioId);
    }

    public async Task<Huespede?> GetByDocumentoAsync(string tipoDocumento, string numeroDocumento)
    {
        return await _dbSet.FirstOrDefaultAsync(h =>
            h.TipoDocumento == tipoDocumento &&
            h.NumeroDocumento == numeroDocumento);
    }

    public async Task<IEnumerable<Huespede>> GetHuespedesVipAsync()
    {
        return await _dbSet.Where(h => h.EsVip).ToListAsync();
    }

    public async Task<IEnumerable<Huespede>> GetByNacionalidadAsync(string nacionalidad)
    {
        return await _dbSet.Where(h => h.Nacionalidad == nacionalidad).ToListAsync();
    }
}
