using Microsoft.EntityFrameworkCore;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class DepartamentoRepository : GenericRepository<Departamento>, IDepartamentoRepository
{
    public DepartamentoRepository(BarceloIoTSystemContext context) : base(context)
    {
    }

    public async Task<Departamento?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Nombre == nombre);
    }

    public async Task<IEnumerable<Departamento>> GetActivosAsync()
    {
        return await _dbSet.Where(d => d.EstaActivo).ToListAsync();
    }
}
