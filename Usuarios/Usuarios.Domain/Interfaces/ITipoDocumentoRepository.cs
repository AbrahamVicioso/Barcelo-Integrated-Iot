using Usuarios.Domain.Entities;

namespace Usuarios.Domain.Interfaces;

public interface ITipoDocumentoRepository : IGenericRepository<TipoDocumento>
{
    Task<TipoDocumento?> GetByNombreAsync(string nombre);
    Task<IEnumerable<TipoDocumento>> GetActivosAsync();
}
