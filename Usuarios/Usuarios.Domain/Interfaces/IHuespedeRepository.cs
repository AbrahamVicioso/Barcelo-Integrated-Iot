using Usuarios.Domain.Entities;

namespace Usuarios.Domain.Interfaces;

public interface IHuespedeRepository : IGenericRepository<Huespede>
{
    Task<Huespede?> GetByUsuarioIdAsync(string usuarioId);
    Task<Huespede?> GetByDocumentoAsync(int tipoDocumentoId, string numeroDocumento);
    Task<IEnumerable<Huespede>> GetHuespedesVipAsync();
    Task<IEnumerable<Huespede>> GetByNacionalidadAsync(string nacionalidad);
}
