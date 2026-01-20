using Usuarios.Domain.Entities;

namespace Usuarios.Domain.Interfaces;

public interface IPersonalRepository : IGenericRepository<Personal>
{
    Task<Personal?> GetByUsuarioIdAsync(string usuarioId);
    Task<Personal?> GetByNumeroEmpleadoAsync(string numeroEmpleado);
    Task<IEnumerable<Personal>> GetByHotelIdAsync(int hotelId);
    Task<IEnumerable<Personal>> GetByDepartamentoAsync(string departamento);
    Task<IEnumerable<Personal>> GetPersonalActivoAsync();
    Task<IEnumerable<Personal>> GetBySupervisorAsync(int supervisorId);
    Task<Personal?> GetWithPermisosAsync(int personalId);
}
