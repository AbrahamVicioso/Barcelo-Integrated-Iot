using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BarceloIoTSystemContext _context;
    private IHuespedeRepository? _huespedeRepository;
    private IPersonalRepository? _personalRepository;
    private IPermisosPersonalRepository? _permisosPersonalRepository;

    public UnitOfWork(BarceloIoTSystemContext context)
    {
        _context = context;
    }

    public IHuespedeRepository Huespedes
    {
        get
        {
            _huespedeRepository ??= new HuespedeRepository(_context);
            return _huespedeRepository;
        }
    }

    public IPersonalRepository Personal
    {
        get
        {
            _personalRepository ??= new PersonalRepository(_context);
            return _personalRepository;
        }
    }

    public IPermisosPersonalRepository PermisosPersonal
    {
        get
        {
            _permisosPersonalRepository ??= new PermisosPersonalRepository(_context);
            return _permisosPersonalRepository;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
