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

    private IQueryable<PermisosPersonal> WithIncludes()
        => _dbSet
            .Include(p => p.Personal)
            .Include(p => p.Habitacion)
            .Include(p => p.Actividad)
            .Include(p => p.OtorgadoPorNavigation);

    public override async Task<PermisosPersonal?> GetByIdAsync(int id)
    {
        return await WithIncludes().FirstOrDefaultAsync(p => p.PermisoId == id);
    }

    public override async Task<IEnumerable<PermisosPersonal>> GetAllAsync()
    {
        return await WithIncludes().ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetByPersonalIdAsync(int personalId)
    {
        return await WithIncludes()
            .Where(p => p.PersonalId == personalId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosActivosAsync(int personalId)
    {
        return await WithIncludes()
            .Where(p => p.PersonalId == personalId && p.EstaActivo)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosByHabitacionAsync(int habitacionId)
    {
        return await WithIncludes()
            .Where(p => p.HabitacionId == habitacionId && p.EstaActivo)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosByActividadAsync(int actividadId)
    {
        return await WithIncludes()
            .Where(p => p.ActividadId == actividadId && p.EstaActivo)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosTemporalesAsync()
    {
        return await WithIncludes()
            .Where(p => p.EsTemporal)
            .ToListAsync();
    }

    public async Task<IEnumerable<PermisosPersonal>> GetPermisosExpiradosAsync()
    {
        var now = DateTime.UtcNow;
        return await WithIncludes()
            .Where(p => p.FechaExpiracion.HasValue && p.FechaExpiracion.Value < now)
            .ToListAsync();
    }
}
