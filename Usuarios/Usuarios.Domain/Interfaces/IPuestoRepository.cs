using Usuarios.Domain.Entities;

namespace Usuarios.Domain.Interfaces;

public interface IPuestoRepository : IGenericRepository<Puesto>
{
    Task<Puesto?> GetByNombreAsync(string nombre);
    Task<IEnumerable<Puesto>> GetActivosAsync();
}
