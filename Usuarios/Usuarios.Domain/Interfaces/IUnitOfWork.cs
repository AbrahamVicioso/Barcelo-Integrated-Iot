namespace Usuarios.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IHuespedeRepository Huespedes { get; }
    IPersonalRepository Personal { get; }
    IPermisosPersonalRepository PermisosPersonal { get; }
    Task<int> SaveChangesAsync();
}
