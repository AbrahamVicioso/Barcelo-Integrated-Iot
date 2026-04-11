using Usuarios.Domain.Entities;

namespace Usuarios.Domain.Interfaces;

public interface IDepartamentoRepository : IGenericRepository<Departamento>
{
    Task<Departamento?> GetByNombreAsync(string nombre);
    Task<IEnumerable<Departamento>> GetActivosAsync();
}
