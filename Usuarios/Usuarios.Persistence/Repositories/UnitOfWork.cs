using Usuarios.Domain.Interfaces;
using Usuarios.Persistence.Data;

namespace Usuarios.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BarceloIoTSystemContext _context;
    private IHuespedeRepository? _huespedeRepository;
    private IPersonalRepository? _personalRepository;
    private IPermisosPersonalRepository? _permisosPersonalRepository;
    private IPuestoRepository? _puestoRepository;
    private IDepartamentoRepository? _departamentoRepository;

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

    public IPuestoRepository Puestos
    {
        get
        {
            _puestoRepository ??= new PuestoRepository(_context);
            return _puestoRepository;
        }
    }

    public IDepartamentoRepository Departamentos
    {
        get
        {
            _departamentoRepository ??= new DepartamentoRepository(_context);
            return _departamentoRepository;
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
