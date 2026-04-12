namespace Usuarios.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IHuespedeRepository Huespedes { get; }
    IPersonalRepository Personal { get; }
    IPermisosPersonalRepository PermisosPersonal { get; }
    IPuestoRepository Puestos { get; }
    IDepartamentoRepository Departamentos { get; }
    ITipoDocumentoRepository TiposDocumento { get; }
    Task<int> SaveChangesAsync();
}
