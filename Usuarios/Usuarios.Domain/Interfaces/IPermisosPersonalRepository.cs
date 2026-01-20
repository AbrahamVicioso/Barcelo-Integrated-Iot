using Usuarios.Domain.Entities;

namespace Usuarios.Domain.Interfaces;

public interface IPermisosPersonalRepository : IGenericRepository<PermisosPersonal>
{
    Task<IEnumerable<PermisosPersonal>> GetByPersonalIdAsync(int personalId);
    Task<IEnumerable<PermisosPersonal>> GetPermisosActivosAsync(int personalId);
    Task<IEnumerable<PermisosPersonal>> GetPermisosByHabitacionAsync(int habitacionId);
    Task<IEnumerable<PermisosPersonal>> GetPermisosTemporalesAsync();
    Task<IEnumerable<PermisosPersonal>> GetPermisosExpiradosAsync();
}
