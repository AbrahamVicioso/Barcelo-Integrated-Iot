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

    public override async Task<IEnumerable<Huespede>> GetAllAsync()
    {
        return await _dbSet.Include(h => h.TipoDocumentoNavigation).ToListAsync();
    }

    public override async Task<Huespede?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(h => h.TipoDocumentoNavigation).FirstOrDefaultAsync(h => h.HuespedId == id);
    }

    public async Task<Huespede?> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _dbSet.Include(h => h.TipoDocumentoNavigation)
            .FirstOrDefaultAsync(h => h.UsuarioId == usuarioId);
    }

    public async Task<Huespede?> GetByDocumentoAsync(int tipoDocumentoId, string numeroDocumento)
    {
        return await _dbSet.FirstOrDefaultAsync(h =>
            h.TipoDocumentoId == tipoDocumentoId &&
            h.NumeroDocumento == numeroDocumento);
    }

    public async Task<IEnumerable<Huespede>> GetHuespedesVipAsync()
    {
        return await _dbSet.Include(h => h.TipoDocumentoNavigation).Where(h => h.EsVip).ToListAsync();
    }

    public async Task<IEnumerable<Huespede>> GetByNacionalidadAsync(string nacionalidad)
    {
        return await _dbSet.Include(h => h.TipoDocumentoNavigation)
            .Where(h => h.Nacionalidad == nacionalidad).ToListAsync();
    }
}
