using Microsoft.EntityFrameworkCore;
using Usuarios.Domain.Entities;
using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class PermisosPersonalRepository : GenericRepository<PermisosPersonal>, IPermisosPersonalRepository
{
    public PermisosPersonalRepository(BarceloIoTSystemContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PermisosPersonal>> GetByPersonalIdAsync(int personalId)
    {
        return await _dbSet.Where(p => p.PersonalId == personalId).ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosActivosAsync(int personalId)
    {
        return await _dbSet
            .Where(p => p.PersonalId == personalId && p.EstaActivo)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosByHabitacionAsync(int habitacionId)
    {
        return await _dbSet
            .Where(p => p.HabitacionId == habitacionId && p.EstaActivo)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosTemporalesAsync()
    {
        return await _dbSet.Where(p => p.EsTemporal).ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosExpiradosAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(p => p.FechaExpiracion.HasValue && p.FechaExpiracion.Value < now)
            .ToListAsync();
    }
}
